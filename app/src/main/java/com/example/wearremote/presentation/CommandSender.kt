package com.example.wearremote.presentation

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.OkHttpClient
import okhttp3.Request
import java.util.concurrent.TimeUnit

object CommandSender {

    private val http = OkHttpClient.Builder()
        .connectTimeout(5, TimeUnit.SECONDS)
        .readTimeout(5, TimeUnit.SECONDS)
        .build()

    suspend fun send(ip: String, port: String, auth: String, cmd: String): String =
        withContext(Dispatchers.IO) {
            try {
                val url = "http://$ip:$port/api?cmd=$cmd&auth=$auth"
                val resp = http.newCall(Request.Builder().url(url).get().build()).execute()
                if (resp.isSuccessful) "OK" else "Ошибка: ${resp.code}"
            } catch (e: java.net.ConnectException) {
                "Нет соединения"
            } catch (e: java.net.SocketTimeoutException) {
                "Таймаут"
            } catch (e: Exception) {
                "Ошибка: ${e.message?.take(50)}"
            }
        }
}
