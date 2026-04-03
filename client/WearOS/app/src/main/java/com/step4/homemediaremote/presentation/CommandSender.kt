package com.step4.homemediaremote.presentation

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlinx.serialization.encodeToString
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import java.util.concurrent.TimeUnit

object CommandSender {

    /** Общий экземпляр Json — используйте для ручной сериализации */
    val json = Json { encodeDefaults = true; ignoreUnknownKeys = true }

    private val http = OkHttpClient.Builder()
        .connectTimeout(5, TimeUnit.SECONDS)
        .readTimeout(5, TimeUnit.SECONDS)
        .build()

    private val JSON_TYPE = "application/json; charset=utf-8".toMediaType()

    /**
     *  POST http://{host}:{port}/api/{command}
     *  Header:  X-API-AUTH: {auth}
     *  Body:    {jsonBody}
     */
    suspend fun send(
        host: String,
        port: String,
        auth: String,
        command: String,
        jsonBody: String = "{}"
    ): String = withContext(Dispatchers.IO) {
        try {
            val url = "http://$host:$port/api/$command"
            val body = jsonBody.toRequestBody(JSON_TYPE)
            val request = Request.Builder()
                .url(url)
                .post(body)
                .addHeader("X-API-AUTH", auth)
                .build()
            val resp = http.newCall(request).execute()
            if (resp.isSuccessful) "OK" else "Ошибка: ${resp.code}"
        } catch (e: java.net.ConnectException) { "Нет соединения"
        } catch (e: java.net.SocketTimeoutException) { "Таймаут"
        } catch (e: Exception) { "Ошибка: ${e.message?.take(50)}" }
    }

    /** Типизированная отправка — сериализует body автоматически */
    suspend inline fun <reified T> sendBody(
        host: String, port: String, auth: String,
        command: String, body: T
    ): String = send(host, port, auth, command, json.encodeToString(body))
}

/** Удобная сериализация: `MyBody(...).toJson()` */
inline fun <reified T> T.toJson(): String = CommandSender.json.encodeToString(this)
