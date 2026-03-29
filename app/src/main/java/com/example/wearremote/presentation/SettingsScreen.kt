package com.example.wearremote.presentation

import android.app.Activity
import android.content.Intent
import android.text.InputType
import android.widget.Toast
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.wear.compose.foundation.lazy.ScalingLazyColumn
import androidx.wear.compose.foundation.lazy.rememberScalingLazyListState
import androidx.wear.compose.material.Button
import androidx.wear.compose.material.Chip
import androidx.wear.compose.material.ChipDefaults
import androidx.wear.compose.material.MaterialTheme
import androidx.wear.compose.material.Text
import kotlinx.coroutines.launch

@Composable
fun SettingsScreen(
    dataStore: SettingsDataStore,
    onNavigateToRemote: () -> Unit
) {
    val context = LocalContext.current
    val scope = rememberCoroutineScope()

    val savedIp by dataStore.ipAddress.collectAsState(initial = "")
    val savedPort by dataStore.port.collectAsState(initial = "")
    val savedAuth by dataStore.authCode.collectAsState(initial = "")

    var ipInput by remember(savedIp) { mutableStateOf(savedIp) }
    var portInput by remember(savedPort) { mutableStateOf(savedPort) }
    var authInput by remember(savedAuth) { mutableStateOf(savedAuth) }

    var editingField by remember { mutableStateOf<String?>(null) }

    val inputLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == Activity.RESULT_OK) {
            val value = result.data?.getStringExtra(InputActivity.EXTRA_RESULT) ?: ""
            when (editingField) {
                "ip"   -> ipInput = value
                "port" -> portInput = value
                "auth" -> authInput = value
            }
        }
        editingField = null
    }

    fun openInput(
        field: String,
        label: String,
        value: String,
        inputType: Int = InputType.TYPE_CLASS_NUMBER
    ) {
        editingField = field
        inputLauncher.launch(
            Intent(context, InputActivity::class.java).apply {
                putExtra(InputActivity.EXTRA_LABEL, label)
                putExtra(InputActivity.EXTRA_VALUE, value)
                putExtra(InputActivity.EXTRA_INPUT_TYPE, inputType)
            }
        )
    }

    val listState = rememberScalingLazyListState()

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black)
    ) {
        ScalingLazyColumn(
            state = listState,
            modifier = Modifier.fillMaxSize(),
            contentPadding = PaddingValues(
                start = 16.dp, end = 16.dp, top = 40.dp, bottom = 40.dp
            ),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            item {
                Text(
                    text = "Настройки",
                    style = MaterialTheme.typography.title3,
                    color = MaterialTheme.colors.primary
                )
                Spacer(Modifier.height(4.dp))
            }

            if (savedIp.isNotEmpty()) {
                item {
                    Chip(
                        label = { Text("Пульт →") },
                        onClick = onNavigateToRemote,
                        colors = ChipDefaults.primaryChipColors(),
                        modifier = Modifier.fillMaxWidth()
                    )
                    Spacer(Modifier.height(4.dp))
                }
            }

            item {
                Chip(
                    label = { Text("Адрес сервера") },
                    secondaryLabel = { Text(ipInput.ifEmpty { "нажмите для ввода" }) },
                    onClick = {
                        openInput(
                            "ip",
                            "Адрес сервера",
                            ipInput,
                            inputType = InputType.TYPE_CLASS_TEXT or InputType.TYPE_TEXT_VARIATION_URI
                        )
                    },
                    colors = ChipDefaults.secondaryChipColors(
                        backgroundColor = Color(0xFF404040),
                        contentColor = Color.White
                    ),
                    modifier = Modifier.fillMaxWidth()
                )
            }

            item {
                Chip(
                    label = { Text("Порт") },
                    secondaryLabel = { Text(portInput.ifEmpty { "нажмите для ввода" }) },
                    onClick = { openInput("port", "Порт", portInput) },
                    colors = ChipDefaults.secondaryChipColors(
                        backgroundColor = Color(0xFF404040),
                        contentColor = Color.White
                    ),
                    modifier = Modifier.fillMaxWidth()
                )
            }

            item {
                Chip(
                    label = { Text("Код авторизации") },
                    secondaryLabel = { Text(authInput.ifEmpty { "нажмите для ввода" }) },
                    onClick = { openInput("auth", "Код авторизации", authInput) },
                    colors = ChipDefaults.secondaryChipColors(
                        backgroundColor = Color(0xFF404040),
                        contentColor = Color.White
                    ),
                    modifier = Modifier.fillMaxWidth()
                )
            }

            item {
                Spacer(Modifier.height(8.dp))
                Button(
                    onClick = {
                        scope.launch {
                            dataStore.saveSettings(ipInput, portInput, authInput)
                            Toast.makeText(context, "Сохранено!", Toast.LENGTH_SHORT).show()
                            onNavigateToRemote()
                        }
                    },
                    modifier = Modifier.fillMaxWidth(0.75f)
                ) { Text("Сохранить ✓") }
            }
        }
    }
}
