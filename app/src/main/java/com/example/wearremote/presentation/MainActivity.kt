package com.example.wearremote.presentation

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.platform.LocalContext
import androidx.wear.compose.material.MaterialTheme
import androidx.wear.compose.navigation.SwipeDismissableNavHost
import androidx.wear.compose.navigation.composable
import androidx.wear.compose.navigation.rememberSwipeDismissableNavController

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                WearRemoteApp()
            }
        }
    }
}

@Composable
fun WearRemoteApp() {
    val context = LocalContext.current
    val dataStore = remember { SettingsDataStore(context) }
    val navController = rememberSwipeDismissableNavController()

    SwipeDismissableNavHost(
        navController = navController,
        startDestination = "remote"  // всегда стартуем с настроек
    ) {

        // ---- Экран настроек ----
        composable("settings") {
            SettingsScreen(
                dataStore = dataStore,
                onNavigateToRemote = {
                    navController.navigate("remote")
                }
            )
        }

        // ---- Экран пульта ----
        composable("remote") {
            RemoteScreen(
                dataStore = dataStore,
                onOpenSettings = {
                    navController.popBackStack()   // возврат к настройкам
                }
            )
        }
    }
}
