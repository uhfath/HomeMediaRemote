package com.step4.homemediaremote.presentation

import android.content.Intent
import android.graphics.Color
import android.graphics.drawable.GradientDrawable
import android.os.Bundle
import android.text.InputType
import android.view.Gravity
import android.view.inputmethod.EditorInfo
import android.widget.EditText
import android.widget.LinearLayout
import android.widget.TextView
import androidx.activity.ComponentActivity

class InputActivity : ComponentActivity() {

    companion object {
        const val EXTRA_LABEL = "label"
        const val EXTRA_VALUE = "value"
        const val EXTRA_INPUT_TYPE = "inputType"
        const val EXTRA_ALLOW_DOTS = "allowDots"
        const val EXTRA_RESULT = "result"
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val label = intent.getStringExtra(EXTRA_LABEL) ?: ""
        val currentValue = intent.getStringExtra(EXTRA_VALUE) ?: ""
        val inputTypeFlag = intent.getIntExtra(EXTRA_INPUT_TYPE, InputType.TYPE_CLASS_NUMBER)
        val allowDots = intent.getBooleanExtra(EXTRA_ALLOW_DOTS, false)

        // ── Разметка: Label + EditText, всё вверху экрана ──
        val layout = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            gravity = Gravity.CENTER_HORIZONTAL
            setPadding(32, 48, 32, 16)
            setBackgroundColor(Color.BLACK)
        }

        val labelView = TextView(this).apply {
            text = label
            setTextColor(Color.GRAY)
            textSize = 14f
            gravity = Gravity.CENTER
        }

        val editText = EditText(this).apply {
            // --- Тип клавиатуры ---
            inputType = inputTypeFlag

            // --- Внешний вид ---
            background = GradientDrawable().apply {
                setStroke(2, Color.GRAY)
                cornerRadius = 24f
                setColor(Color.TRANSPARENT)
            }
            setTextColor(Color.WHITE)
            setHintTextColor(Color.GRAY)
            setPadding(32, 20, 32, 20)
            textSize = 20f
            isSingleLine = true
            gravity = Gravity.CENTER
            imeOptions = EditorInfo.IME_ACTION_DONE

            // --- Начальное значение ---
            setText(currentValue)
            setSelection(text.length)

            // --- Кнопка "Done" на клавиатуре → вернуть результат ---
            setOnEditorActionListener { _, actionId, _ ->
                if (actionId == EditorInfo.IME_ACTION_DONE) {
                    returnResult(text.toString())
                    true
                } else false
            }
        }

        layout.addView(labelView, LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.MATCH_PARENT,
            LinearLayout.LayoutParams.WRAP_CONTENT
        ))

        layout.addView(editText, LinearLayout.LayoutParams(
            LinearLayout.LayoutParams.MATCH_PARENT,
            LinearLayout.LayoutParams.WRAP_CONTENT
        ).apply {
            topMargin = 12
        })

        setContentView(layout)
        editText.requestFocus()
    }

    private fun returnResult(value: String) {
        setResult(RESULT_OK, Intent().putExtra(EXTRA_RESULT, value))
        finish()
    }
}
