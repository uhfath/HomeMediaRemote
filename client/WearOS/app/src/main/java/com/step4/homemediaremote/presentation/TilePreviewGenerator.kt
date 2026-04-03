package com.step4.homemediaremote.presentation

import android.content.Context
import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Path
import android.os.Handler
import android.os.Looper
import android.util.Log
import android.view.View
import android.widget.FrameLayout
import androidx.core.content.ContextCompat
import androidx.wear.protolayout.LayoutElementBuilders
import androidx.wear.protolayout.ResourceBuilders
import java.io.File
import java.util.concurrent.CountDownLatch

@Suppress("DEPRECATION")
object TilePreviewGenerator {

    private const val SIZE_PX = 384

    fun generate(context: Context) {
        val dir = File(context.filesDir, "tile_previews")
        if (!dir.exists()) dir.mkdirs()

        TileDefs.ALL.forEach { def ->
            val layout    = TileLayoutBuilder.buildLayout(def, context.packageName)
            val resources = TileLayoutBuilder.buildResources(def)
            val bmp = renderOnMainThread(context, layout, resources)
            File(dir, "tile_preview_${def.id}.png").outputStream().use {
                bmp.compress(Bitmap.CompressFormat.PNG, 100, it)
            }
            bmp.recycle()
        }
        Log.d("TilePreview", "Превью сохранены: ${dir.absolutePath}")
    }

    /* ════════════════════════════════════════════════════════════
     *  protolayout wrapper  →  tiles wrapper
     *  Без хардкода proto-классов: всё через рефлексию.
     *
     *  Стратегия 1 — прямая передача proto-объекта
     *    (tiles 1.4+ использует protolayout-proto внутри,
     *     поэтому fromProto() часто принимает его напрямую)
     *
     *  Стратегия 2 — байтовый round-trip
     *    toByteArray() → parseFrom() для нужного proto-типа
     * ════════════════════════════════════════════════════════════ */

    @Suppress("UNCHECKED_CAST")
    private fun <T> convertToTiles(src: Any, tilesClass: Class<T>): T {
        // src.toProto() → protobuf-объект (protolayout proto)
        val proto = src.javaClass.getMethod("toProto").invoke(src)!!

        // Все перегрузки fromProto(), отсортированные по числу параметров
        val methods = tilesClass.declaredMethods
            .filter { it.name == "fromProto" }
            .sortedBy { it.parameterCount }
            .onEach { it.isAccessible = true }

        // Стратегия 1: proto-объект подходит по типу → передаём как есть
        for (m in methods) {
            if (m.parameterTypes[0].isInstance(proto)) {
                val args = Array(m.parameterCount) { i ->
                    if (i == 0) proto else null
                }
                return m.invoke(null, *args) as T
            }
        }

        // Стратегия 2: сериализуем в байты, парсим в ожидаемый proto-класс
        val bytes = proto.javaClass
            .getMethod("toByteArray")
            .invoke(proto) as ByteArray

        for (m in methods) {
            runCatching {
                val expectedClass = m.parameterTypes[0]
                val parsed = expectedClass
                    .getMethod("parseFrom", ByteArray::class.java)
                    .invoke(null, bytes)!!
                val args = Array(m.parameterCount) { i ->
                    if (i == 0) parsed else null
                }
                return m.invoke(null, *args) as T
            }
        }

        error("Cannot convert ${src.javaClass.name} → ${tilesClass.name}")
    }

    private fun toTilesLayout(
        layout: LayoutElementBuilders.Layout
    ): androidx.wear.tiles.LayoutElementBuilders.Layout =
        convertToTiles(layout,
            androidx.wear.tiles.LayoutElementBuilders.Layout::class.java)

    private fun toTilesResources(
        resources: ResourceBuilders.Resources
    ): androidx.wear.tiles.ResourceBuilders.Resources =
        convertToTiles(resources,
            androidx.wear.tiles.ResourceBuilders.Resources::class.java)

    /* ═══════ rendering ═══════ */

    private fun renderOnMainThread(
        context: Context,
        layout: LayoutElementBuilders.Layout,
        resources: ResourceBuilders.Resources
    ): Bitmap {
        if (Looper.myLooper() == Looper.getMainLooper()) {
            return doRender(context, layout, resources)
        }
        val latch = CountDownLatch(1)
        var result: Bitmap? = null
        Handler(Looper.getMainLooper()).post {
            result = doRender(context, layout, resources)
            latch.countDown()
        }
        latch.await()
        return result!!
    }

    private fun doRender(
        context: Context,
        layout: LayoutElementBuilders.Layout,
        resources: ResourceBuilders.Resources
    ): Bitmap {
        val parent = FrameLayout(context)

        val renderer = androidx.wear.tiles.renderer.TileRenderer(
            context,
            toTilesLayout(layout),
            toTilesResources(resources),
            ContextCompat.getMainExecutor(context)
        ) { /* LoadActionListener — no-op */ }
        renderer.inflate(parent)

        val spec = View.MeasureSpec.makeMeasureSpec(SIZE_PX, View.MeasureSpec.EXACTLY)
        parent.measure(spec, spec)
        parent.layout(0, 0, SIZE_PX, SIZE_PX)

        val bmp = Bitmap.createBitmap(SIZE_PX, SIZE_PX, Bitmap.Config.ARGB_8888)
        val canvas = Canvas(bmp)
        canvas.clipPath(Path().apply {
            addCircle(SIZE_PX / 2f, SIZE_PX / 2f, SIZE_PX / 2f, Path.Direction.CW)
        })
        canvas.drawColor(Color.BLACK)
        parent.draw(canvas)
        return bmp
    }
}
