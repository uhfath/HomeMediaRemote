package com.example.wearremote.presentation

import android.content.Context
import android.os.VibrationEffect
import android.os.Vibrator
import android.widget.Toast
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.focusable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.pager.HorizontalPager
import androidx.compose.foundation.pager.rememberPagerState
import androidx.compose.foundation.shape.CircleShape
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
import androidx.compose.ui.input.rotary.onRotaryScrollEvent
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalView
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.wear.compose.material.Chip
import androidx.wear.compose.material.ChipDefaults
import androidx.wear.compose.material.HorizontalPageIndicator
import androidx.wear.compose.material.MaterialTheme
import androidx.wear.compose.material.PageIndicatorState
import androidx.wear.compose.material.Text
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.launch
import kotlin.math.abs
import androidx.compose.foundation.gestures.detectVerticalDragGestures
import androidx.compose.ui.input.pointer.pointerInput

// ═══════════════════════════════════════════════════════
//  Константы
// ═══════════════════════════════════════════════════════

const val PAGE_COUNT = 5
private const val ROTARY_THRESHOLD = 30f   // чувствительность безеля

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

    // ── Авто-команда от плитки (выполняется один раз) ──
    var autoSent by remember { mutableStateOf(false) }
    LaunchedEffect(autoCommand, host) {
        if (autoCommand != null && host.isNotEmpty() && !autoSent) {
            autoSent = true
            val r = CommandSender.send(host, port, auth, autoCommand)
            if (r.startsWith("OK")) vibrateOk(context) else vibrateErr(context)
            Toast.makeText(context, r, Toast.LENGTH_SHORT).show()
        }
    }

    fun cmd(command: String) {
        scope.launch {
            val r = CommandSender.send(host, port, auth, command)
            if (r.startsWith("OK")) vibrateOk(context) else vibrateErr(context)
            Toast.makeText(context, r, Toast.LENGTH_SHORT).show()
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
            // ── Свайп вверх → настройки ──
            .pointerInput(Unit) {
                while (true) {
                    var totalY = 0f
                    detectVerticalDragGestures(
                        onDragStart = { totalY = 0f },
                        onDragEnd = {
                            if (totalY < -80f) onOpenSettings()
                            totalY = 0f
                        },
                        onVerticalDrag = { _, dy -> totalY += dy }
                    )
                }
            }
    ) {
        HorizontalPager(state = pagerState, modifier = Modifier.fillMaxSize()) { page ->
            val isCurrent = pagerState.currentPage == page
            when (page) {
                0 -> MediaPage(::cmd, onOpenSettings)
                1 -> SoundPage(isCurrent, ::cmd, onOpenSettings)
                2 -> MicPage(isCurrent, ::cmd, onOpenSettings)
                3 -> ComputerPage(::cmd, onOpenSettings)
                4 -> ScreenPage(::cmd, onOpenSettings)
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
            modifier = Modifier.align(Alignment.BottomCenter).padding(bottom = 1.dp)
        )
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 0 — Медиа
// ═══════════════════════════════════════════════════════

@Composable
private fun MediaPage(cmd: (String) -> Unit, onSettings: () -> Unit) {
    PageShell(title = "🎵 Медиа", onSettings = onSettings) {
        BtnRow {
            CmdChip("⏮ Пред", cmd = { cmd("media_prev") })
            CmdChip("⏭ След", cmd = { cmd("media_next") })
        }
        BtnRow {
            CmdChip("▶ Play",   cmd = { cmd("media_play") })
            CmdChip("⏸ Пауза", cmd = { cmd("media_pause") })
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 1 — Звук (безель = громкость)
// ═══════════════════════════════════════════════════════

@Composable
private fun SoundPage(isCurrent: Boolean, cmd: (String) -> Unit, onSettings: () -> Unit) {
    RotaryPageShell(
        title = "🔊 Звук",
        hint = "⟳ Безель: громкость",
        isCurrent = isCurrent,
        onRotaryUp = { cmd("vol_up") },
        onRotaryDown = { cmd("vol_down") },
        onSettings = onSettings
    ) {
        BtnRow {
            CmdChip("🔇 Выкл", cmd = { cmd("sound_mute") })
            CmdChip("🔊 Вкл",  cmd = { cmd("sound_unmute") })
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 2 — Микрофон (безель = чувствительность)
// ═══════════════════════════════════════════════════════

@Composable
private fun MicPage(isCurrent: Boolean, cmd: (String) -> Unit, onSettings: () -> Unit) {
    RotaryPageShell(
        title = "🎤 Микрофон",
        hint = "⟳ Безель: чувствительность",
        isCurrent = isCurrent,
        onRotaryUp = { cmd("mic_sens_up") },
        onRotaryDown = { cmd("mic_sens_down") },
        onSettings = onSettings
    ) {
        BtnRow {
            CmdChip("🔇 Выкл", cmd = { cmd("mic_off") })
            CmdChip("🎤 Вкл",  cmd = { cmd("mic_on") })
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 3 — Компьютер
// ═══════════════════════════════════════════════════════

@Composable
private fun ComputerPage(cmd: (String) -> Unit, onSettings: () -> Unit) {
    PageShell(title = "💻 Компьютер", onSettings = onSettings) {
        BtnRow {
            CmdChip("🔒 Блок", cmd = { cmd("pc_lock") })
            CmdChip("💤 Сон",  cmd = { cmd("pc_sleep") })
        }
        BtnRow {
            CmdChip("🔄 Рест",  cmd = { cmd("pc_restart") })
            CmdChip("⏻ Выкл", cmd = { cmd("pc_shutdown") })
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Страница 4 — Экран
// ═══════════════════════════════════════════════════════

@Composable
private fun ScreenPage(cmd: (String) -> Unit, onSettings: () -> Unit) {
    PageShell(title = "🖥 Экран", onSettings = onSettings) {
        BtnRow {
            CmdChip("💡 Вкл",  cmd = { cmd("screen_on") })
            CmdChip("🌙 Выкл", cmd = { cmd("screen_off") })
        }
    }
}

// ═══════════════════════════════════════════════════════
//  Каркас обычной страницы
// ═══════════════════════════════════════════════════════

@Composable
private fun PageShell(
    title: String,
    onSettings: () -> Unit,
    content: @Composable () -> Unit
) {
    Box(
        modifier = Modifier.fillMaxSize().background(Color.Black),
        contentAlignment = Alignment.Center
    ) {
        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.spacedBy(4.dp)
        ) {
            Text(title, style = MaterialTheme.typography.title3)
            Spacer(Modifier.height(4.dp))
            content()
        }
        SettingsBtn(Modifier.align(Alignment.BottomCenter), onSettings)
    }
}

// ═══════════════════════════════════════════════════════
//  Каркас страницы с безелем
// ═══════════════════════════════════════════════════════

@Composable
private fun RotaryPageShell(
    title: String,
    hint: String,
    isCurrent: Boolean,
    onRotaryUp: () -> Unit,
    onRotaryDown: () -> Unit,
    onSettings: () -> Unit,
    content: @Composable () -> Unit
) {
    val focusRequester = remember { FocusRequester() }
    var rotaryAccum by remember { mutableStateOf(0f) }
    var feedback by remember { mutableStateOf("") }

    LaunchedEffect(isCurrent) {
        if (isCurrent) focusRequester.requestFocus()
    }

    // Сбрасываем подсказку через секунду
    LaunchedEffect(feedback) {
        if (feedback.isNotEmpty()) { delay(800); feedback = "" }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
            .onRotaryScrollEvent { event ->
                rotaryAccum += event.verticalScrollPixels
                if (abs(rotaryAccum) >= ROTARY_THRESHOLD) {
                    if (rotaryAccum > 0) {
                        onRotaryUp(); feedback = "▲"
                    } else {
                        onRotaryDown(); feedback = "▼"
                    }
                    rotaryAccum = 0f
                }
                true
            }
            .focusRequester(focusRequester)
            .focusable(),
        contentAlignment = Alignment.Center
    ) {
        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.spacedBy(4.dp)
        ) {
            Text(title, style = MaterialTheme.typography.title3)
            Text(
                text = if (feedback.isNotEmpty()) feedback else hint,
                fontSize = 11.sp,
                color = if (feedback.isNotEmpty()) Color.White else Color.Gray
            )
            Spacer(Modifier.height(2.dp))
            content()
        }
        SettingsBtn(Modifier.align(Alignment.BottomCenter), onSettings)
    }
}

// ═══════════════════════════════════════════════════════
//  Переиспользуемые компоненты
// ═══════════════════════════════════════════════════════

@Composable
private fun BtnRow(content: @Composable RowScope.() -> Unit) {
    Row(
        modifier = Modifier.fillMaxWidth().padding(horizontal = 12.dp),
        horizontalArrangement = Arrangement.spacedBy(4.dp),
        content = content
    )
}

@Composable
private fun RowScope.CmdChip(label: String, cmd: () -> Unit) {
    Chip(
        label = { Text(label, maxLines = 1, fontSize = 12.sp) },
        onClick = cmd,
        colors = ChipDefaults.secondaryChipColors(
            backgroundColor = Color(0xFF404040),
            contentColor = Color.White
        ),
        modifier = Modifier.weight(1f)
    )
}

@Composable
private fun SettingsBtn(modifier: Modifier, onClick: () -> Unit) {
    Box(
        modifier = modifier
            .padding(bottom = 12.dp)
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
