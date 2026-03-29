package com.example.wearremote.presentation

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

// Делегат DataStore — должен быть объявлен на верхнем уровне файла (вне класса)
private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "settings")

class SettingsDataStore(private val context: Context) {

    companion object {
        private val IP_ADDRESS_KEY = stringPreferencesKey("ip_address")
        private val PORT_KEY = stringPreferencesKey("port")
        private val AUTH_CODE_KEY = stringPreferencesKey("auth_code")
    }

    // Потоки для чтения сохранённых значений
    val ipAddress: Flow<String> = context.dataStore.data.map { prefs ->
        prefs[IP_ADDRESS_KEY] ?: ""
    }

    val port: Flow<String> = context.dataStore.data.map { prefs ->
        prefs[PORT_KEY] ?: ""
    }

    val authCode: Flow<String> = context.dataStore.data.map { prefs ->
        prefs[AUTH_CODE_KEY] ?: ""
    }

    // Функция сохранения всех настроек
    suspend fun saveSettings(ip: String, port: String, authCode: String) {
        context.dataStore.edit { prefs ->
            prefs[IP_ADDRESS_KEY] = ip
            prefs[PORT_KEY] = port
            prefs[AUTH_CODE_KEY] = authCode
        }
    }
}
