package com.example.wearremote.presentation

import androidx.concurrent.futures.CallbackToFutureAdapter
import androidx.wear.protolayout.LayoutElementBuilders
import androidx.wear.protolayout.ResourceBuilders
import androidx.wear.protolayout.TimelineBuilders
import androidx.wear.tiles.RequestBuilders
import androidx.wear.tiles.TileBuilders
import androidx.wear.tiles.TileService
import com.google.common.util.concurrent.ListenableFuture

// ═══════════════════════════════════════════════════════
//  Базовый класс — делегирует в TileLayoutBuilder
// ═══════════════════════════════════════════════════════

abstract class BaseRemoteTile : TileService() {

    abstract val def: TilePageDef

    override fun onTileRequest(
        req: RequestBuilders.TileRequest
    ): ListenableFuture<TileBuilders.Tile> =
        CallbackToFutureAdapter.getFuture { cb ->
            val layout = TileLayoutBuilder.buildLayout(def, packageName)
            cb.set(
                TileBuilders.Tile.Builder()
                    .setResourcesVersion("1")
                    .setTileTimeline(
                        TimelineBuilders.Timeline.Builder().addTimelineEntry(
                            TimelineBuilders.TimelineEntry.Builder()
                                .setLayout(layout).build()
                        ).build()
                    ).build()
            )
            "tile"
        }

    override fun onTileResourcesRequest(
        req: RequestBuilders.ResourcesRequest
    ): ListenableFuture<ResourceBuilders.Resources> =
        CallbackToFutureAdapter.getFuture { cb ->
            cb.set(TileLayoutBuilder.buildResources(def))
            "res"
        }
}

// ═══════════════════════════════════════════════════════
//  Конкретные плитки
// ═══════════════════════════════════════════════════════

class MediaTile    : BaseRemoteTile() { override val def = TileDefs.MEDIA    }
class SoundTile    : BaseRemoteTile() { override val def = TileDefs.SOUND    }
class MicTile      : BaseRemoteTile() { override val def = TileDefs.MIC      }
class ComputerTile : BaseRemoteTile() { override val def = TileDefs.COMPUTER }
class ScreenTile   : BaseRemoteTile() { override val def = TileDefs.SCREEN   }
