package com.example.wearremote.presentation

import android.content.Context
import android.os.VibrationEffect
import android.os.Vibrator
import android.widget.Toast
import androidx.compose.animation.animateColorAsState
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalView
import androidx.compose.ui.unit.dp
import androidx.wear.compose.foundation.lazy.ScalingLazyColumn
import androidx.wear.compose.material.Button
import androidx.wear.compose.material.ButtonDefaults
import androidx.wear.compose.material.MaterialTheme
import androidx.wear.compose.material.Text
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import okhttp3.OkHttpClient
import okhttp3.Request
import java.util.concurrent.TimeUnit

// ─────────────────────────────────────────────────
// Статус соединения
// ─────────────────────────────────────────────────

enum class ConnectionStatus { UNKNOWN, OK, ERROR }

// ─────────────────────────────────────────────────
// HTTP-клиент (единственный экземпляр)
// ─────────────────────────────────────────────────

private val httpClient = OkHttpClient.Builder()
    .connectTimeout(5, TimeUnit.SECONDS)
    .readTimeout(5, TimeUnit.SECONDS)
    .build()

// ─────────────────────────────────────────────────
// Вибрация
// ─────────────────────────────────────────────────

private fun vibrateSuccess(context: Context) {
    val vibrator = context.getSystemService(Vibrator::class.java) ?: return
    vibrator.vibrate(
        VibrationEffect.createOneShot(50, VibrationEffect.DEFAULT_AMPLITUDE)
    )
}

private fun vibrateError(context: Context) {
    val vibrator = context.getSystemService(Vibrator::class.java) ?: return
    vibrator.vibrate(
        VibrationEffect.createWaveform(longArrayOf(0, 80, 60, 80), -1)
    )
}

// ─────────────────────────────────────────────────
// Экран пульта
// ─────────────────────────────────────────────────

@Composable
fun RemoteScreen(
    dataStore: SettingsDataStore,
    onOpenSettings: () -> Unit
) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()

    // ---- Не гасить экран пока открыт пульт ----
    val view = LocalView.current
    DisposableEffect(Unit) {
        view.keepScreenOn = true
        onDispose { view.keepScreenOn = false }
    }

    // ---- Данные из DataStore ----
    val ip by dataStore.ipAddress.collectAsState(initial = "")
    val port by dataStore.port.collectAsState(initial = "")
    val authCode by dataStore.authCode.collectAsState(initial = "")

    // ---- Состояние ----
    var status by remember { mutableStateOf(ConnectionStatus.UNKNOWN) }
    var isLoading by remember { mutableStateOf(false) }

    // ---- Отправка команды ----
    fun executeCommand(command: String) {
        if (isLoading) return
        scope.launch {
            isLoading = true
            val result = sendCommand(ip, port, authCode, command)

            if (result.startsWith("OK")) {
                status = ConnectionStatus.OK
                vibrateSuccess(context)
            } else {
                status = ConnectionStatus.ERROR
                vibrateError(context)
            }

            Toast.makeText(context, result, Toast.LENGTH_SHORT).show()
            isLoading = false
        }
    }

    // ---- UI ----
    ScalingLazyColumn(
        modifier = Modifier.fillMaxSize(),
        contentPadding = PaddingValues(
            start = 16.dp,
            end = 16.dp,
            top = 40.dp,
            bottom = 40.dp
        ),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        // ---- Заголовок + статус ----
        item {
            Text(
                text = "WearRemote",
                style = MaterialTheme.typography.title3,
                color = MaterialTheme.colors.primary
            )
        }
        item {
            StatusRow(ip = ip, port = port, status = status)
            Spacer(Modifier.height(8.dp))
        }

        // ──────────────────────────────────────────
        // Кнопки команд
        // ──────────────────────────────────────────

        item {
            RemoteButton("▶  Play", isLoading) { executeCommand("play") }
        }
        item {
            RemoteButton("⏸  Pause", isLoading) { executeCommand("pause") }
        }
        item {
            RemoteButton("⏭  Next", isLoading) { executeCommand("next") }
        }
        item {
            RemoteButton("⏮  Prev", isLoading) { executeCommand("prev") }
        }
        item {
            RemoteButton("🔊  Vol +", isLoading) { executeCommand("vol_up") }
        }
        item {
            RemoteButton("🔉  Vol −", isLoading) { executeCommand("vol_down") }
        }

        // ---- Настройки ----
        item {
            Spacer(Modifier.height(12.dp))
            Button(
                onClick = onOpenSettings,
                colors = ButtonDefaults.secondaryButtonColors(),
                modifier = Modifier.fillMaxWidth(0.7f)
            ) {
                Text("⚙ Настройки")
            }
        }
    }
}

// ─────────────────────────────────────────────────
// Строка статуса: цветной кружок + адрес
// ─────────────────────────────────────────────────

@Composable
fun StatusRow(ip: String, port: String, status: ConnectionStatus) {
    val dotColor by animateColorAsState(
        targetValue = when (status) {
            ConnectionStatus.UNKNOWN -> Color.Gray
            ConnectionStatus.OK      -> Color(0xFF4CAF50)  // зелёный
            ConnectionStatus.ERROR   -> Color(0xFFF44336)  // красный
        },
        label = "statusColor"
    )

    val statusText = when (status) {
        ConnectionStatus.UNKNOWN -> "нет данных"
        ConnectionStatus.OK      -> "подключён"
        ConnectionStatus.ERROR   -> "ошибка"
    }

    Row(
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.Center
    ) {
        Box(
            modifier = Modifier
                .size(8.dp)
                .clip(CircleShape)
                .background(dotColor)
        )
        Spacer(Modifier.width(6.dp))
        Text(
            text = "$ip:$port · $statusText",
            style = MaterialTheme.typography.caption2,
            color = Color.Gray
        )
    }
}

// ─────────────────────────────────────────────────
// Кнопка пульта с блокировкой при загрузке
// ─────────────────────────────────────────────────

@Composable
fun RemoteButton(label: String, isLoading: Boolean, onClick: () -> Unit) {
    Button(
        onClick = onClick,
        enabled = !isLoading,
        modifier = Modifier
            .fillMaxWidth(0.8f)
            .padding(vertical = 2.dp)
    ) {
        Text(if (isLoading) "⏳" else label)
    }
}

// ─────────────────────────────────────────────────
// HTTP-запрос (фоновый поток)
// ─────────────────────────────────────────────────

suspend fun sendCommand(
    ip: String,
    port: String,
    auth: String,
    command: String
): String {
    return withContext(Dispatchers.IO) {
        try {
            val url = "http://$ip:$port/api?cmd=$command&auth=$auth"
            val request = Request.Builder().url(url).get().build()
            val response = httpClient.newCall(request).execute()

            if (response.isSuccessful) {
                val body = response.body?.string()?.take(80) ?: ""
                "OK: $body"
            } else {
                "Ошибка: HTTP ${response.code}"
            }
        } catch (e: java.net.ConnectException) {
            "Нет соединения"
        } catch (e: java.net.SocketTimeoutException) {
            "Таймаут"
        } catch (e: Exception) {
            "Ошибка: ${e.message?.take(50)}"
        }
    }
}
