package com.example.wearremote.presentation

import androidx.concurrent.futures.CallbackToFutureAdapter
import androidx.wear.protolayout.ActionBuilders
import androidx.wear.protolayout.ColorBuilders
import androidx.wear.protolayout.DimensionBuilders
import androidx.wear.protolayout.LayoutElementBuilders
import androidx.wear.protolayout.ModifiersBuilders
import androidx.wear.protolayout.ResourceBuilders
import androidx.wear.protolayout.TimelineBuilders
import androidx.wear.tiles.RequestBuilders
import androidx.wear.tiles.TileBuilders
import androidx.wear.tiles.TileService
import com.google.common.util.concurrent.ListenableFuture

// ═══════════════════════════════════════════════════════
data class TileBtn(val label: String, val command: String)

// ═══════════════════════════════════════════════════════
//  Базовый класс — только примитивы ProtoLayout
// ═══════════════════════════════════════════════════════

abstract class BaseRemoteTile : TileService() {

    abstract val pageIndex: Int
    abstract val label: String
    abstract val icon: String
    abstract val buttons: List<TileBtn>

    // ── TileService callbacks ──

    override fun onTileRequest(
        req: RequestBuilders.TileRequest
    ): ListenableFuture<TileBuilders.Tile> =
        CallbackToFutureAdapter.getFuture { it.set(buildTile()); "tile" }

    override fun onTileResourcesRequest(
        req: RequestBuilders.ResourcesRequest
    ): ListenableFuture<ResourceBuilders.Resources> =
        CallbackToFutureAdapter.getFuture {
            it.set(ResourceBuilders.Resources.Builder().setVersion("1").build()); "res"
        }

    // ── Построение плитки ──

    private fun buildTile(): TileBuilders.Tile {

        val column = LayoutElementBuilders.Column.Builder()
            .setHorizontalAlignment(LayoutElementBuilders.HORIZONTAL_ALIGN_CENTER)
            .addContent(text("$icon $label", 16f, 0xFFBBDEFB))
            .addContent(vSpacer(10f))

        // Кнопки по 2 в ряд
        buttons.chunked(2).forEach { row ->
            val rowBuilder = LayoutElementBuilders.Row.Builder()
                .setVerticalAlignment(LayoutElementBuilders.VERTICAL_ALIGN_CENTER)
            row.forEachIndexed { i, btn ->
                if (i > 0) rowBuilder.addContent(hSpacer(6f))
                rowBuilder.addContent(chipBox(btn.label, btn.command))
            }
            column.addContent(rowBuilder.build())
            column.addContent(vSpacer(6f))
        }

        // Корневой Box: чёрный фон, тап по пустому месту → открыть приложение
        val root = LayoutElementBuilders.Box.Builder()
            .setWidth(DimensionBuilders.expand())
            .setHeight(DimensionBuilders.expand())
            .setVerticalAlignment(LayoutElementBuilders.VERTICAL_ALIGN_CENTER)
            .setHorizontalAlignment(LayoutElementBuilders.HORIZONTAL_ALIGN_CENTER)
            .setModifiers(
                ModifiersBuilders.Modifiers.Builder()
                    .setClickable(openAppClickable())
                    .setBackground(
                        ModifiersBuilders.Background.Builder()
                            .setColor(argb(0xFF000000))
                            .build()
                    )
                    .build()
            )
            .addContent(column.build())
            .build()

        return TileBuilders.Tile.Builder()
            .setResourcesVersion("1")
            .setTileTimeline(
                TimelineBuilders.Timeline.Builder().addTimelineEntry(
                    TimelineBuilders.TimelineEntry.Builder().setLayout(
                        LayoutElementBuilders.Layout.Builder()
                            .setRoot(root).build()
                    ).build()
                ).build()
            ).build()
    }

    // ── Кнопка-чип (Box + Text + скруглённый фон) ──

    private fun chipBox(
        label: String,
        command: String
    ): LayoutElementBuilders.LayoutElement =
        LayoutElementBuilders.Box.Builder()
            .setModifiers(
                ModifiersBuilders.Modifiers.Builder()
                    .setClickable(commandClickable(command))
                    .setBackground(
                        ModifiersBuilders.Background.Builder()
                            .setColor(argb(0xFF404040))
                            .setCorner(
                                ModifiersBuilders.Corner.Builder()
                                    .setRadius(DimensionBuilders.dp(14f))
                                    .build()
                            )
                            .build()
                    )
                    .setPadding(pad(12f, 10f))
                    .build()
            )
            .addContent(text(label, 14f, 0xFFFFFFFF))
            .build()

    // ── Clickable: открыть приложение на нужной странице ──

    private fun openAppClickable(): ModifiersBuilders.Clickable =
        ModifiersBuilders.Clickable.Builder()
            .setId("open")
            .setOnClick(launchApp(pageIndex, null))
            .build()

    // ── Clickable: открыть + выполнить команду ──

    private fun commandClickable(command: String): ModifiersBuilders.Clickable =
        ModifiersBuilders.Clickable.Builder()
            .setId("cmd_$command")
            .setOnClick(launchApp(pageIndex, command))
            .build()

    private fun launchApp(page: Int, cmd: String?): ActionBuilders.LaunchAction {
        val ab = ActionBuilders.AndroidActivity.Builder()
            .setPackageName(packageName)
            .setClassName("$packageName.presentation.MainActivity")
            .addKeyToExtraMapping(
                "open_page",
                ActionBuilders.AndroidIntExtra.Builder().setValue(page).build()
            )
        if (cmd != null) {
            ab.addKeyToExtraMapping(
                "auto_cmd",
                ActionBuilders.AndroidStringExtra.Builder().setValue(cmd).build()
            )
        }
        return ActionBuilders.LaunchAction.Builder()
            .setAndroidActivity(ab.build())
            .build()
    }

    // ── Примитивы ──

    private fun text(t: String, sp: Float, color: Long) =
        LayoutElementBuilders.Text.Builder()
            .setText(t)
            .setFontStyle(
                LayoutElementBuilders.FontStyle.Builder()
                    .setSize(DimensionBuilders.sp(sp))
                    .setColor(argb(color))
                    .build()
            )
            .build()

    private fun vSpacer(dp: Float) =
        LayoutElementBuilders.Spacer.Builder()
            .setHeight(DimensionBuilders.dp(dp)).build()

    private fun hSpacer(dp: Float) =
        LayoutElementBuilders.Spacer.Builder()
            .setWidth(DimensionBuilders.dp(dp)).build()

    private fun argb(c: Long) =
        ColorBuilders.ColorProp.Builder(c.toInt()).build()

    private fun pad(h: Float, v: Float) =
        ModifiersBuilders.Padding.Builder()
            .setStart(DimensionBuilders.dp(h))
            .setEnd(DimensionBuilders.dp(h))
            .setTop(DimensionBuilders.dp(v))
            .setBottom(DimensionBuilders.dp(v))
            .build()
}

// ═══════════════════════════════════════════════════════
//  Конкретные плитки
// ═══════════════════════════════════════════════════════

class MediaTile : BaseRemoteTile() {
    override val pageIndex = 0; override val label = "Медиа"; override val icon = "🎵"
    override val buttons = listOf(
        TileBtn("⏮", "media_prev"),  TileBtn("▶", "media_play"),
        TileBtn("⏸", "media_pause"), TileBtn("⏭", "media_next")
    )
}

class SoundTile : BaseRemoteTile() {
    override val pageIndex = 1; override val label = "Звук"; override val icon = "🔊"
    override val buttons = listOf(
        TileBtn("🔇", "sound_mute"),  TileBtn("🔊", "sound_unmute"),
        TileBtn("−", "vol_down"),     TileBtn("+", "vol_up")
    )
}

class MicTile : BaseRemoteTile() {
    override val pageIndex = 2; override val label = "Микрофон"; override val icon = "🎤"
    override val buttons = listOf(
        TileBtn("🔇", "mic_off"),      TileBtn("🎤", "mic_on"),
        TileBtn("−", "mic_sens_down"), TileBtn("+", "mic_sens_up")
    )
}

class ComputerTile : BaseRemoteTile() {
    override val pageIndex = 3; override val label = "Компьютер"; override val icon = "💻"
    override val buttons = listOf(
        TileBtn("🔒", "pc_lock"),    TileBtn("💤", "pc_sleep"),
        TileBtn("🔄", "pc_restart"), TileBtn("⏻", "pc_shutdown")
    )
}

class ScreenTile : BaseRemoteTile() {
    override val pageIndex = 4; override val label = "Экран"; override val icon = "🖥"
    override val buttons = listOf(
        TileBtn("💡", "screen_on"), TileBtn("🌙", "screen_off")
    )
}
