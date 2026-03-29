package com.example.wearremote.presentation.tiles

import androidx.concurrent.futures.CallbackToFutureAdapter
import androidx.wear.protolayout.ActionBuilders
import androidx.wear.protolayout.ColorBuilders
import androidx.wear.protolayout.LayoutElementBuilders
import androidx.wear.protolayout.ModifiersBuilders
import androidx.wear.protolayout.ResourceBuilders
import androidx.wear.protolayout.TimelineBuilders
import androidx.wear.protolayout.material.CompactChip
import androidx.wear.protolayout.material.Text
import androidx.wear.protolayout.material.Typography
import androidx.wear.protolayout.material.layouts.PrimaryLayout
import androidx.wear.tiles.RequestBuilders
import androidx.wear.tiles.TileBuilders
import androidx.wear.tiles.TileService
import com.google.common.util.concurrent.ListenableFuture

// ═══════════════════════════════════════════════════════
//  Базовый класс — общая логика для всех плиток
// ═══════════════════════════════════════════════════════

abstract class BaseRemoteTile : TileService() {

    abstract val pageIndex: Int
    abstract val label: String
    abstract val icon: String

    override fun onTileRequest(
        requestParams: RequestBuilders.TileRequest
    ): ListenableFuture<TileBuilders.Tile> =
        CallbackToFutureAdapter.getFuture { completer ->
            completer.set(buildTile(requestParams))
            "onTileRequest"
        }

    override fun onTileResourcesRequest(
        requestParams: RequestBuilders.ResourcesRequest
    ): ListenableFuture<ResourceBuilders.Resources> =
        CallbackToFutureAdapter.getFuture { completer ->
            completer.set(
                ResourceBuilders.Resources.Builder().setVersion("1").build()
            )
            "onResources"
        }

    private fun buildTile(req: RequestBuilders.TileRequest): TileBuilders.Tile {
        val deviceParams = req.deviceConfiguration

        // Действие при нажатии: открыть приложение на нужной странице
        val clickable = ModifiersBuilders.Clickable.Builder()
            .setId("open_page_$pageIndex")
            .setOnClick(
                ActionBuilders.LaunchAction.Builder()
                    .setAndroidActivity(
                        ActionBuilders.AndroidActivity.Builder()
                            .setPackageName(packageName)
                            .setClassName(
                                "$packageName.presentation.MainActivity"
                            )
                            .addKeyToExtraMapping(
                                "open_page",
                                ActionBuilders.AndroidIntExtra.Builder()
                                    .setValue(pageIndex)
                                    .build()
                            )
                            .build()
                    )
                    .build()
            )
            .build()

        val layout = PrimaryLayout.Builder(deviceParams)
            .setContent(
                Text.Builder(this, "$icon\n$label")
                    .setTypography(Typography.TYPOGRAPHY_TITLE3)
                    .setColor(
                        ColorBuilders.ColorProp.Builder(0xFFBBDEFB.toInt())
                            .build()
                    )
                    .setMultilineAlignment(
                        LayoutElementBuilders.TEXT_ALIGN_CENTER
                    )
                    .build()
            )
            .setPrimaryChipContent(
                CompactChip.Builder(this, "Открыть", clickable, deviceParams)
                    .build()
            )
            .build()

        return TileBuilders.Tile.Builder()
            .setResourcesVersion("1")
            .setTileTimeline(
                TimelineBuilders.Timeline.Builder()
                    .addTimelineEntry(
                        TimelineBuilders.TimelineEntry.Builder()
                            .setLayout(
                                LayoutElementBuilders.Layout.Builder()
                                    .setRoot(layout)
                                    .build()
                            ).build()
                    ).build()
            ).build()
    }
}

// ═══════════════════════════════════════════════════════
//  Конкретные плитки — по одной строке
// ═══════════════════════════════════════════════════════

class MediaTile      : BaseRemoteTile() { override val pageIndex=0; override val label="Медиа";     override val icon="🎵" }
class SoundTile      : BaseRemoteTile() { override val pageIndex=1; override val label="Звук";      override val icon="🔊" }
class MicTile        : BaseRemoteTile() { override val pageIndex=2; override val label="Микрофон";  override val icon="🎤" }
class ComputerTile   : BaseRemoteTile() { override val pageIndex=3; override val label="Компьютер"; override val icon="💻" }
class ScreenTile     : BaseRemoteTile() { override val pageIndex=4; override val label="Экран";     override val icon="🖥" }
