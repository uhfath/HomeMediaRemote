package com.example.wearremote.presentation

import android.content.Context
import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Paint
import android.graphics.Path
import android.graphics.RectF
import android.graphics.Typeface
import android.util.Log
import java.io.File

object TilePreviewGenerator {

    private data class Info(
        val id: String, val icon: String, val label: String,
        val buttons: List<List<String>>
    )

    private val TILES = listOf(
        Info("media",    "🎵", "Медиа",     listOf(listOf("⏮️","▶️"), listOf("⏸️","⏭️"))),
        Info("sound",    "🔊", "Звук",      listOf(listOf("🔇","🔊"), listOf("➖","➕"))),
        Info("mic",      "🎤", "Микрофон",  listOf(listOf("🔇","🎤"), listOf("➖","➕"))),
        Info("computer", "💻", "Компьютер", listOf(listOf("🔒","💤"), listOf("🔄","🔌"))),
        Info("screen",   "🖥", "Экран",     listOf(listOf("💡","🌙")))
    )

    fun generate(context: Context) {
        val dir = File(context.filesDir, "tile_previews")
        if (!dir.exists())
            dir.mkdirs()

        TILES.forEach { tile ->
            val bmp = render(tile)
            File(dir, "tile_preview_${tile.id}.png").outputStream().use {
                bmp.compress(Bitmap.CompressFormat.PNG, 100, it)
            }
            bmp.recycle()
        }
        Log.d("TilePreview", "Превью сохранены: ${dir.absolutePath}")
    }

    private fun render(tile: Info): Bitmap {
        val s = 384
        val bmp = Bitmap.createBitmap(s, s, Bitmap.Config.ARGB_8888)
        val c = Canvas(bmp)

        c.clipPath(Path().apply { addCircle(s / 2f, s / 2f, s / 2f, Path.Direction.CW) })
        c.drawColor(Color.BLACK)

        val titleP = Paint(Paint.ANTI_ALIAS_FLAG).apply {
            color = 0xFFBBDEFB.toInt(); textSize = 38f
            textAlign = Paint.Align.CENTER; typeface = Typeface.DEFAULT_BOLD
        }
        c.drawText("${tile.icon} ${tile.label}", s / 2f, 120f, titleP)

        val bgP = Paint(Paint.ANTI_ALIAS_FLAG).apply { color = 0xFF404040.toInt() }
        val txP = Paint(Paint.ANTI_ALIAS_FLAG).apply {
            color = Color.WHITE; textSize = 36f; textAlign = Paint.Align.CENTER
        }

        val margin = 55f; val gap = 14f; val bh = 66f; val sy = 155f

        tile.buttons.forEachIndexed { ri, row ->
            val y = sy + ri * (bh + gap)
            val tw = s - 2 * margin
            val bw = (tw - (row.size - 1) * gap) / row.size
            row.forEachIndexed { ci, label ->
                val x = margin + ci * (bw + gap)
                val r = RectF(x, y, x + bw, y + bh)
                c.drawRoundRect(r, 24f, 24f, bgP)
                c.drawText(label, r.centerX(),
                    r.centerY() - (txP.descent() + txP.ascent()) / 2, txP)
            }
        }
        return bmp
    }
}
