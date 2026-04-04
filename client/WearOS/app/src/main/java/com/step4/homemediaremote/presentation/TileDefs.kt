package com.step4.homemediaremote.presentation

import com.step4.homemediaremote.R

// ═══════════════════════════════════════════════════════
//  Общие определения всех плиток / страниц
// ═══════════════════════════════════════════════════════

data class TileBtnDef(
    val iconRes: Int,       // R.drawable.ic_xxx
    val command: String,
    val resourceId: String  // ключ для ProtoLayout-ресурса
)

data class TilePageDef(
    val id: String,
    val pageIndex: Int,
    val label: String,
    val buttons: List<TileBtnDef>
)

object TileDefs {

    val MEDIA = TilePageDef("media", 0, "Медиа", listOf(
        TileBtnDef(R.drawable.ic_pause,              "media_pause", "ic_pause"),
        TileBtnDef(R.drawable.ic_play_arrow,         "media_play",  "ic_play"),
        TileBtnDef(R.drawable.ic_skip_previous,      "media_prev",  "ic_skip_prev"),
        TileBtnDef(R.drawable.ic_skip_next,          "media_next",  "ic_skip_next")
    ))

    val SOUND = TilePageDef("sound", 1, "Звук", listOf(
        TileBtnDef(R.drawable.ic_volume_off,  "sound_mute",   "ic_vol_off"),
        TileBtnDef(R.drawable.ic_volume_up,   "sound_unmute", "ic_vol_up"),
        TileBtnDef(R.drawable.ic_remove,      "vol_down",     "ic_minus"),
        TileBtnDef(R.drawable.ic_add,         "vol_up",       "ic_plus")
    ))

    val MIC = TilePageDef("mic", 2, "Микрофон", listOf(
        TileBtnDef(R.drawable.ic_mic_off, "mic_off",       "ic_mic_off"),
        TileBtnDef(R.drawable.ic_mic,     "mic_on",        "ic_mic"),
        TileBtnDef(R.drawable.ic_remove,  "mic_sens_down", "ic_minus2"),
        TileBtnDef(R.drawable.ic_add,     "mic_sens_up",   "ic_plus2")
    ))

    val COMPUTER = TilePageDef("computer", 4, "Компьютер", listOf(
        TileBtnDef(R.drawable.ic_bedtime,            "pc_sleep",    "ic_sleep"),
        TileBtnDef(R.drawable.ic_lock,               "pc_lock",     "ic_lock"),
        TileBtnDef(R.drawable.ic_power_settings_new, "pc_shutdown", "ic_power"),
        TileBtnDef(R.drawable.ic_refresh,            "pc_restart",  "ic_restart"),
    ))

    val ALL = listOf(MEDIA, SOUND, MIC, COMPUTER)
}
