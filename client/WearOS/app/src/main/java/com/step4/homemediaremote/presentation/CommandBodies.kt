package com.step4.homemediaremote.presentation

import kotlinx.serialization.Serializable

// ── Медиа ──
@Serializable class MediaPlayBody(val position: Long = 0)
@Serializable class MediaPauseBody
@Serializable class MediaNextBody
@Serializable class MediaPrevBody

// ── Звук ──
@Serializable class SoundMuteBody
@Serializable class SoundUnmuteBody
@Serializable class VolumeUpBody(val step: Int = 1)
@Serializable class VolumeDownBody(val step: Int = 1)

// ── Микрофон ──
@Serializable class MicOnBody
@Serializable class MicOffBody
@Serializable class MicSensUpBody(val step: Int = 1)
@Serializable class MicSensDownBody(val step: Int = 1)

// ── Компьютер ──
@Serializable class PcLockBody
@Serializable class PcSleepBody
@Serializable class PcRestartBody(val force: Boolean = false)
@Serializable class PcShutdownBody(val force: Boolean = false)

// ── Экран ──
@Serializable class ScreenOnBody
@Serializable class ScreenOffBody
