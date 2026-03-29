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

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        val tileTargetPage = intent.getIntExtra("open_page", -1)
        setContent {
            MaterialTheme { WearRemoteApp(tileTargetPage) }
        }
    }
}

@Composable
fun WearRemoteApp(tileTargetPage: Int = -1) {
    val context = LocalContext.current
    val dataStore = remember { SettingsDataStore(context) }

    // null = загрузка, "settings" / "remote"
    var screen by remember { mutableStateOf<String?>(null) }

    LaunchedEffect(Unit) {
        val ip = dataStore.ipAddress.first()
        screen = if (ip.isNotEmpty()) "remote" else "settings"
    }

    when (screen) {
        "settings" -> SettingsScreen(
            dataStore = dataStore,
            onNavigateToRemote = { screen = "remote" }
        )
        "remote" -> RemotePager(
            dataStore = dataStore,
            targetPage = if (tileTargetPage >= 0) tileTargetPage else null,
            onOpenSettings = { screen = "settings" }
        )
    }
}
