package com.example.wearremote.presentation.tiles

import androidx.concurrent.futures.CallbackToFutureAdapter
import androidx.wear.protolayout.ActionBuilders
import androidx.wear.protolayout.ColorBuilders
import androidx.wear.protolayout.LayoutElementBuilders
import androidx.wear.protolayout.ModifiersBuilders
import androidx.wear.protolayout.ResourceBuilders
import androidx.wear.protolayout.TimelineBuilders
import androidx.wear.protolayout.material.Button
import androidx.wear.protolayout.material.ButtonColors
import androidx.wear.protolayout.material.Text
import androidx.wear.protolayout.material.Typography
import androidx.wear.protolayout.material.layouts.MultiButtonLayout
import androidx.wear.protolayout.material.layouts.PrimaryLayout
import androidx.wear.tiles.RequestBuilders
import androidx.wear.tiles.TileBuilders
import androidx.wear.tiles.TileService
import com.google.common.util.concurrent.ListenableFuture

// ═══════════════════════════════════════════════════════
//  Описание кнопки плитки
// ═══════════════════════════════════════════════════════

data class TileBtn(val label: String, val command: String)

// ═══════════════════════════════════════════════════════
//  Базовый класс — строит плитку из pageIndex + buttons
// ═══════════════════════════════════════════════════════

abstract class BaseRemoteTile : TileService() {

    abstract val pageIndex: Int
    abstract val label: String
    abstract val icon: String
    abstract val buttons: List<TileBtn>

    override fun onTileRequest(
        req: RequestBuilders.TileRequest
    ): ListenableFuture<TileBuilders.Tile> =
        CallbackToFutureAdapter.getFuture { it.set(buildTile(req)); "tile" }

    override fun onTileResourcesRequest(
        req: RequestBuilders.ResourcesRequest
    ): ListenableFuture<ResourceBuilders.Resources> =
        CallbackToFutureAdapter.getFuture {
            it.set(ResourceBuilders.Resources.Builder().setVersion("1").build()); "res"
        }

    private fun buildTile(req: RequestBuilders.TileRequest): TileBuilders.Tile {
        val dp = req.deviceConfiguration

        // ── Заголовок ──
        val title = Text.Builder(this, "$icon $label")
            .setTypography(Typography.TYPOGRAPHY_CAPTION1)
            .setColor(ColorBuilders.ColorProp.Builder(0xFFBBDEFB.toInt()).build())
            .build()

        // ── Сетка кнопок ──
        val grid = MultiButtonLayout.Builder()
        buttons.forEach { btn ->
            grid.addButtonContent(
                Button.Builder(this, makeClickable(btn.command))
                    .setTextContent(btn.label)
                    .setButtonColors(ButtonColors(0xFF404040.toInt(), 0xFFFFFFFF.toInt()))
                    .build()
            )
        }

        val layout = PrimaryLayout.Builder(dp)
            .setPrimaryLabelTextContent(title)
            .setContent(grid.build())
            .build()

        return TileBuilders.Tile.Builder()
            .setResourcesVersion("1")
            .setTileTimeline(
                TimelineBuilders.Timeline.Builder().addTimelineEntry(
                    TimelineBuilders.TimelineEntry.Builder().setLayout(
                        LayoutElementBuilders.Layout.Builder().setRoot(layout).build()
                    ).build()
                ).build()
            ).build()
    }

    /** Нажатие на кнопку → открыть приложение на нужной странице + выполнить команду */
    private fun makeClickable(command: String): ModifiersBuilders.Clickable =
        ModifiersBuilders.Clickable.Builder()
            .setId("cmd_$command")
            .setOnClick(
                ActionBuilders.LaunchAction.Builder()
                    .setAndroidActivity(
                        ActionBuilders.AndroidActivity.Builder()
                            .setPackageName(packageName)
                            .setClassName("$packageName.presentation.MainActivity")
                            .addKeyToExtraMapping(
                                "open_page",
                                ActionBuilders.AndroidIntExtra.Builder()
                                    .setValue(pageIndex).build()
                            )
                            .addKeyToExtraMapping(
                                "auto_cmd",
                                ActionBuilders.AndroidStringExtra.Builder()
                                    .setValue(command).build()
                            )
                            .build()
                    ).build()
            ).build()
}

// ═══════════════════════════════════════════════════════
//  Конкретные плитки
// ═══════════════════════════════════════════════════════

class MediaTile : BaseRemoteTile() {
    override val pageIndex = 0; override val label = "Медиа"; override val icon = "🎵"
    override val buttons = listOf(
        TileBtn("⏮", "media_prev"), TileBtn("▶", "media_play"),
        TileBtn("⏸", "media_pause"), TileBtn("⏭", "media_next")
    )
}

class SoundTile : BaseRemoteTile() {
    override val pageIndex = 1; override val label = "Звук"; override val icon = "🔊"
    override val buttons = listOf(
        TileBtn("🔇", "sound_mute"), TileBtn("🔊", "sound_unmute"),
        TileBtn("−", "vol_down"),    TileBtn("+", "vol_up")
    )
}

class MicTile : BaseRemoteTile() {
    override val pageIndex = 2; override val label = "Микрофон"; override val icon = "🎤"
    override val buttons = listOf(
        TileBtn("🔇", "mic_off"),       TileBtn("🎤", "mic_on"),
        TileBtn("−", "mic_sens_down"),  TileBtn("+", "mic_sens_up")
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
