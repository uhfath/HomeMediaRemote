package com.example.wearremote.presentation

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.platform.LocalContext
import androidx.wear.compose.material.MaterialTheme
import kotlinx.coroutines.flow.first
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        //TilePreviewGenerator.generate(this)
        val tileTargetPage = intent.getIntExtra("open_page", -1)
        val autoCommand = intent.getStringExtra("auto_cmd")    // ← NEW
        setContent {
            MaterialTheme {
                WearRemoteApp(tileTargetPage, autoCommand)      // ← NEW
            }
        }
    }
}

@Composable
fun WearRemoteApp(tileTargetPage: Int = -1, autoCommand: String? = null) {
    val context = LocalContext.current
    val dataStore = remember { SettingsDataStore(context) }
    var screen by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(Unit) {
        val ip = dataStore.ipAddress.first()
        screen = if (ip.isNotEmpty()) "remote" else "settings"
    }

    Box(modifier = Modifier.fillMaxSize().background(Color.Black)) {
        when (screen) {
            "settings" -> SettingsScreen(
                dataStore = dataStore,
                onNavigateToRemote = { screen = "remote" }
            )
            "remote" -> RemotePager(
                dataStore = dataStore,
                targetPage = if (tileTargetPage >= 0) tileTargetPage else null,
                autoCommand = autoCommand,
                onOpenSettings = { screen = "settings" }
            )
        }
    }
}

