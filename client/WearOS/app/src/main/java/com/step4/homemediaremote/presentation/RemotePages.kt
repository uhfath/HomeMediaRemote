package com.step4.homemediaremote.presentation

import android.content.Context
import android.os.VibrationEffect
import android.os.Vibrator
import android.widget.Toast
import androidx.annotation.DrawableRes
import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.focusable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxHeight
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.pager.HorizontalPager
import androidx.compose.foundation.pager.rememberPagerState
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.focus.FocusRequester
import androidx.compose.ui.focus.focusRequester
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.ColorFilter
import androidx.compose.ui.input.rotary.onRotaryScrollEvent
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalView
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.wear.compose.material.HorizontalPageIndicator
import androidx.wear.compose.material.MaterialTheme
import androidx.wear.compose.material.PageIndicatorState
import androidx.wear.compose.material.Text
import com.step4.homemediaremote.R
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import kotlin.math.abs

// ═══════════════════════════════════════════════════════
//  Константы
// ═══════════════════════════════════════════════════════

const val PAGE_COUNT = 5
private const val ROTARY_THRESHOLD = 30f

// ═══════════════════════════════════════════════════════
//  Обёртка-пейджер со всеми экранами
// ═══════════════════════════════════════════════════════

@Composable
fun RemotePager(
    dataStore: SettingsDataStore,
    targetPage: Int?,
    autoCommand: String? = null,
    onOpenSettings: () -> Unit
) {
    val view = LocalView.current
    DisposableEffect(Unit) {
        view.keepScreenOn = true
        onDispose { view.keepScreenOn = false }
    }

    var initialPage by remember { mutableStateOf<Int?>(null) }
    LaunchedEffect(Unit) {
        initialPage = targetPage ?: dataStore.lastPage.first()
    }

    val startPage = initialPage ?: return
    RemotePagerContent(startPage, dataStore, autoCommand, onOpenSettings)
}

@Composable
private fun RemotePagerContent(
    startPage: Int,
    dataStore: SettingsDataStore,
    autoCommand: String?,
    onOpenSettings: () -> Unit
) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()

    val host by dataStore.host.collectAsState(initial = "")
    val port by dataStore.port.collectAsState(initial = "")
    val auth by dataStore.authCode.collectAsState(initial = "")

    val pagerState = rememberPagerState(
        initialPage = startPage.coerceIn(0, PAGE_COUNT - 1),
        pageCount = { PAGE_COUNT }
    )

    LaunchedEffect(pagerState.currentPage) {
        dataStore.saveLastPage(pagerState.currentPage)
    }

    // ── Авто-команда от плитки ──
    var autoSent by remember { mutableStateOf(false) }
    LaunchedEffect(autoCommand, host) {
        if (autoCommand != null && host.isNotEmpty() && !autoSent) {
            autoSent = true
            val r = CommandSender.send(host, port, auth, autoCommand)
            if (r.startsWith("OK")) {
                vibrateOk(context)
            } else {
                Toast.makeText(context, r, Toast.LENGTH_SHORT).show()
                vibrateErr(context)
            }
        }
    }

    fun cmd(command: String, jsonBody: String = "{}") {
        scope.launch {
            val r = CommandSender.send(host, port, auth, command, jsonBody)
            if (r.startsWith("OK")) {
                vibrateOk(context)
            } else {
                Toast.makeText(context, r, Toast.LENGTH_SHORT).show()
                vibrateErr(context)
            }
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
    ) {
        HorizontalPager(state = pagerState, modifier = Modifier.fillMaxSize()) { page ->
            val isCurrent = pagerState.currentPage == page
            val onCmd: (String, String) -> Unit = { c, b -> cmd(c, b) }
            when (page) {
                0 -> MediaPage(isCurrent, onCmd, onOpenSettings)
                1 -> SoundPage(isCurrent, onCmd, onOpenSettings)
                2 -> MicPage(isCurrent, onCmd, onOpenSettings)
                3 -> ScreenPage(isCurrent, onCmd, onOpenSettings)
                4 -> ComputerPage(isCurrent, onCmd, onOpenSettings)
            }
        }

        HorizontalPageIndicator(
            pageIndicatorState = remember {
                object : PageIndicatorState {
                    override val pageCount get() = PAGE_COUNT
                    override val selectedPage get() = pagerState.currentPage
                    override val pageOffset get() = pagerState.currentPageOffsetFraction
                }
            },
            modifier = Modifier
                .align(Alignment.BottomCenter)
                .padding(bottom = 1.dp)
        )
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 0 — Медиа
// ═══════════════════════════════════════════════════════

@Composable
private fun MediaPage(isCurrent: Boolean, cmd: (String, String) -> Unit, onSettings: () -> Unit) {
    PageShell(title = "Медиа", isCurrent = isCurrent, onSettings = onSettings) {
        BtnRow {
            IconBtn(R.drawable.ic_pause, Color.White)       { cmd("media/pause", MediaPauseBody().toJson()) }
            IconBtn(R.drawable.ic_play_arrow, Color.White)    { cmd("media/play",  MediaPlayBody().toJson()) }
        }
        BtnRow {
            IconBtn(R.drawable.ic_skip_previous, Color.White) { cmd("media/prev",  MediaPrevBody().toJson()) }
            IconBtn(R.drawable.ic_skip_next, Color.White)   { cmd("media/next",  MediaNextBody().toJson()) }
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 1 — Звук
// ═══════════════════════════════════════════════════════

@Composable
private fun SoundPage(isCurrent: Boolean, cmd: (String, String) -> Unit, onSettings: () -> Unit) {
    RotaryPageShell(
        title = "Звук",
        isCurrent = isCurrent,
        onRotaryUp   = { cmd("sound/vol_up",   VolumeUpBody().toJson()) },
        onRotaryDown = { cmd("sound/vol_down", VolumeDownBody().toJson()) },
        onSettings = onSettings
    ) {
        BtnRow {
            IconBtn(R.drawable.ic_volume_off, Color.White) { cmd("sound/mute",   SoundMuteBody().toJson()) }
            IconBtn(R.drawable.ic_volume_up, Color.White)  { cmd("sound/unmute", SoundUnmuteBody().toJson()) }
        }
        BtnRow {
            IconBtn(R.drawable.ic_remove, Color.White) { cmd("sound/vol_down",  VolumeDownBody().toJson()) }
            IconBtn(R.drawable.ic_add, Color.White)  { cmd("sound/vol_up",      VolumeUpBody().toJson()) }
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 2 — Микрофон
// ═══════════════════════════════════════════════════════

@Composable
private fun MicPage(isCurrent: Boolean, cmd: (String, String) -> Unit, onSettings: () -> Unit) {
    RotaryPageShell(
        title = "Микрофон",
        isCurrent = isCurrent,
        onRotaryUp   = { cmd("mic/sens_up",   MicSensUpBody().toJson()) },
        onRotaryDown = { cmd("mic/sens_down", MicSensDownBody().toJson()) },
        onSettings = onSettings
    ) {
        BtnRow {
            IconBtn(R.drawable.ic_mic_off, Color.White) { cmd("mic/off", MicOffBody().toJson()) }
            IconBtn(R.drawable.ic_mic, Color.White)     { cmd("mic/on",  MicOnBody().toJson()) }
        }
        BtnRow {
            IconBtn(R.drawable.ic_remove, Color.White) { cmd("mic/sens_down",   MicSensDownBody().toJson()) }
            IconBtn(R.drawable.ic_add, Color.White)  { cmd("mic/sens_up",       MicSensUpBody().toJson()) }
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 3 — Компьютер
// ═══════════════════════════════════════════════════════

@Composable
private fun ComputerPage(isCurrent: Boolean, cmd: (String, String) -> Unit, onSettings: () -> Unit) {
    PageShell(title = "Компьютер", isCurrent = isCurrent, onSettings = onSettings) {
        BtnRow {
            IconBtn(R.drawable.ic_bedtime, Color.White) { cmd("pc/sleep",    PcSleepBody().toJson()) }
            IconBtn(R.drawable.ic_lock, Color.White)    { cmd("pc/lock",     PcLockBody().toJson()) }
        }
        BtnRow {
            IconBtn(R.drawable.ic_power_settings_new, Color.White) { cmd("pc/shutdown", PcShutdownBody().toJson()) }
            IconBtn(R.drawable.ic_refresh, Color.White)            { cmd("pc/restart",  PcRestartBody().toJson()) }
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 4 — Экран
// ═══════════════════════════════════════════════════════

@Composable
private fun ScreenPage(isCurrent: Boolean, cmd: (String, String) -> Unit, onSettings: () -> Unit) {
    PageShell(title = "Экран", isCurrent = isCurrent, onSettings = onSettings) {
        BtnRow {
            IconBtn(R.drawable.ic_dark_mode, Color.White)       { cmd("screen/off", ScreenOffBody().toJson()) }
            IconBtn(R.drawable.ic_brightness_high, Color.White) { cmd("screen/on",  ScreenOnBody().toJson()) }
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Каркас обычной страницы
//  Заголовок → [кнопки на всё место] → ⚙
//  Запрашивает фокус и поглощает rotary-события,
//  чтобы отобрать фокус у rotary-страниц при переходе.
// ═══════════════════════════════════════════════════════

@Composable
private fun PageShell(
    title: String,
    isCurrent: Boolean,
    onSettings: () -> Unit,
    content: @Composable ColumnScope.() -> Unit
) {
    val focusRequester = remember { FocusRequester() }

    LaunchedEffect(isCurrent) {
        if (isCurrent) focusRequester.requestFocus()
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
            .onRotaryScrollEvent { true }          // поглощаем без действия
            .focusRequester(focusRequester)
            .focusable(),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Spacer(Modifier.height(10.dp))
        Text(
            text = title,
            fontSize = 14.sp,
            color = Color(0xFFBBDEFB.toInt()),
            style = MaterialTheme.typography.title3
        )

        // ── Кнопки занимают всё оставшееся место ──
        Column(
            modifier = Modifier
                .weight(1f)
                .fillMaxWidth()
                .padding(horizontal = 14.dp, vertical = 6.dp),
            verticalArrangement = Arrangement.spacedBy(6.dp),
            content = content
        )

        SettingsBtn(onClick = onSettings)
        Spacer(Modifier.height(8.dp))
    }
}

// ═══════════════════════════════════════════════════════
//  Каркас страницы с безелем
// ═══════════════════════════════════════════════════════

@Composable
private fun RotaryPageShell(
    title: String,
    isCurrent: Boolean,
    onRotaryUp: () -> Unit,
    onRotaryDown: () -> Unit,
    onSettings: () -> Unit,
    content: @Composable ColumnScope.() -> Unit
) {
    val focusRequester = remember { FocusRequester() }
    var rotaryAccum by remember { mutableStateOf(0f) }
    var feedback by remember { mutableStateOf("") }

    LaunchedEffect(isCurrent) {
        if (isCurrent) focusRequester.requestFocus()
    }
    LaunchedEffect(feedback) {
        if (feedback.isNotEmpty()) { delay(800); feedback = "" }
    }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
            .onRotaryScrollEvent { event ->
                if (!isCurrent) return@onRotaryScrollEvent true   // не текущая — глушим
                rotaryAccum += event.verticalScrollPixels
                if (abs(rotaryAccum) >= ROTARY_THRESHOLD) {
                    if (rotaryAccum > 0) {
                        onRotaryUp(); feedback = "🔼"
                    } else {
                        onRotaryDown(); feedback = "🔽"
                    }
                    rotaryAccum = 0f
                }
                true
            }
            .focusRequester(focusRequester)
            .focusable(),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Spacer(Modifier.height(6.dp))
        Text(
            text = title,
            fontSize = 14.sp,
            color = Color(0xFFBBDEFB.toInt()),
            style = MaterialTheme.typography.title3
        )
        Text(
            text = if (feedback.isNotEmpty()) feedback else "",
            fontSize = 12.sp,
            color = if (feedback.isNotEmpty()) Color.White else Color.Gray
        )

        // ── Кнопки занимают всё оставшееся место ──
        Column(
            modifier = Modifier
                .weight(1f)
                .fillMaxWidth()
                .padding(horizontal = 14.dp, vertical = 4.dp),
            verticalArrangement = Arrangement.spacedBy(6.dp),
            content = content
        )

        SettingsBtn(onClick = onSettings)
        Spacer(Modifier.height(8.dp))
    }
}

// ═══════════════════════════════════════════════════════
//  Переиспользуемые компоненты
// ═══════════════════════════════════════════════════════

/** Ряд кнопок, занимает равную долю вертикального пространства */
@Composable
private fun ColumnScope.BtnRow(content: @Composable RowScope.() -> Unit) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .weight(1f),
        horizontalArrangement = Arrangement.spacedBy(6.dp),
        content = content
    )
}

/** Кнопка-иконка, растягивается на всё доступное место */
@Composable
private fun RowScope.IconBtn(@DrawableRes iconRes: Int, tintColor: Color, onClick: () -> Unit) {
    Box(
        modifier = Modifier
            .weight(1f)
            .fillMaxHeight()
            .clip(RoundedCornerShape(14.dp))
            .background(Color(0x00000000))
            .clickable(onClick = onClick),
        contentAlignment = Alignment.Center
    ) {
        Image(
            painter = painterResource(iconRes),
            contentDescription = null,
            colorFilter = ColorFilter.tint(tintColor),
            modifier = Modifier.size(48.dp)
        )
    }
}

/** Кнопка входа в настройки (⚙) */
@Composable
private fun SettingsBtn(modifier: Modifier = Modifier, onClick: () -> Unit) {
    Box(
        modifier = modifier
            .size(28.dp)
            .clip(CircleShape)
            .background(Color(0xFF333333))
            .clickable(onClick = onClick),
        contentAlignment = Alignment.Center
    ) {
        Text("⚙", fontSize = 12.sp, color = Color.Gray)
    }
}

// ═══════════════════════════════════════════════════════
//  Вибрация
// ═══════════════════════════════════════════════════════

private fun vibrateOk(ctx: Context) {
    ctx.getSystemService(Vibrator::class.java)
        ?.vibrate(VibrationEffect.createOneShot(50, VibrationEffect.DEFAULT_AMPLITUDE))
}

private fun vibrateErr(ctx: Context) {
    ctx.getSystemService(Vibrator::class.java)
        ?.vibrate(VibrationEffect.createWaveform(longArrayOf(0, 80, 60, 80), -1))
}
