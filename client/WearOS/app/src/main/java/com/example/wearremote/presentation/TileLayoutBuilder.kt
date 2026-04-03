package com.example.wearremote.presentation

import androidx.wear.protolayout.ActionBuilders
import androidx.wear.protolayout.ColorBuilders
import androidx.wear.protolayout.DimensionBuilders
import androidx.wear.protolayout.LayoutElementBuilders
import androidx.wear.protolayout.ModifiersBuilders
import androidx.wear.protolayout.ResourceBuilders

object TileLayoutBuilder {

    private const val ICON_DP      = 48f
    private const val CORNER_DP    = 14f
    private const val GAP_DP       = 6f
    private const val PAD_H_DP     = 14f
    private const val PAD_TOP_DP   = 10f
    private const val PAD_BOT_DP   = 10f
    private const val TITLE_SP     = 14f

    // ── Построить Layout ────────────────────────────

    fun buildLayout(
        def: TilePageDef,
        packageName: String
    ): LayoutElementBuilders.Layout =
        LayoutElementBuilders.Layout.Builder()
            .setRoot(buildRoot(def, packageName))
            .build()

    // ── Построить Resources (ссылки на drawable по ID) ──

    fun buildResources(def: TilePageDef): ResourceBuilders.Resources {
        val b = ResourceBuilders.Resources.Builder().setVersion("1")
        val seen = mutableSetOf<String>()
        def.buttons.forEach { btn ->
            if (seen.add(btn.resourceId)) {
                b.addIdToImageMapping(
                    btn.resourceId,
                    ResourceBuilders.ImageResource.Builder()
                        .setAndroidResourceByResId(
                            ResourceBuilders.AndroidImageResourceByResId.Builder()
                                .setResourceId(btn.iconRes)
                                .build()
                        )
                        .build()
                )
            }
        }
        return b.build()
    }

    // ══════════════════════════════════════════════════
    //  Внутренние методы
    // ══════════════════════════════════════════════════

    private fun buildRoot(
        def: TilePageDef,
        pkg: String
    ): LayoutElementBuilders.LayoutElement {

        val col = LayoutElementBuilders.Column.Builder()
            .setWidth(DimensionBuilders.expand())
            .setHeight(DimensionBuilders.expand())
            .setHorizontalAlignment(LayoutElementBuilders.HORIZONTAL_ALIGN_CENTER)
            .setModifiers(padModifiers(h = PAD_H_DP))

        // ── Верхний отступ + заголовок ──
        col.addContent(vSpacer(PAD_TOP_DP))
        col.addContent(titleText(def.label))
        col.addContent(vSpacer(GAP_DP))

        // ── Ряды кнопок (по 2), занимают всё оставшееся место ──
        val rows = def.buttons.chunked(2)
        rows.forEachIndexed { idx, row ->
            col.addContent(buildBtnRow(row, def.pageIndex, pkg))
            if (idx < rows.lastIndex) col.addContent(vSpacer(GAP_DP))
        }

        col.addContent(vSpacer(PAD_BOT_DP))

        // ── Корневой Box: чёрный фон, тап → открыть приложение ──
        return LayoutElementBuilders.Box.Builder()
            .setWidth(DimensionBuilders.expand())
            .setHeight(DimensionBuilders.expand())
            .setModifiers(
                ModifiersBuilders.Modifiers.Builder()
                    .setClickable(openClickable(def.pageIndex, pkg))
                    .setBackground(bgSolid(0x00000000))
                    .build()
            )
            .addContent(col.build())
            .build()
    }

    // ── Ряд кнопок ──────────────────────────────────

    private fun buildBtnRow(
        btns: List<TileBtnDef>,
        page: Int,
        pkg: String
    ): LayoutElementBuilders.LayoutElement {
        val row = LayoutElementBuilders.Row.Builder()
            .setWidth(DimensionBuilders.expand())
            .setHeight(DimensionBuilders.expand())     // делит пространство поровну
            .setVerticalAlignment(LayoutElementBuilders.VERTICAL_ALIGN_CENTER)

        btns.forEachIndexed { i, btn ->
            if (i > 0) row.addContent(hSpacer(GAP_DP))
            row.addContent(btnBox(btn, page, pkg))
        }
        return row.build()
    }

    // ── Одна кнопка: Box + иконка ───────────────────

    private fun btnBox(
        btn: TileBtnDef,
        page: Int,
        pkg: String
    ): LayoutElementBuilders.LayoutElement =
        LayoutElementBuilders.Box.Builder()
            .setWidth(DimensionBuilders.expand())
            .setHeight(DimensionBuilders.expand())
            .setHorizontalAlignment(LayoutElementBuilders.HORIZONTAL_ALIGN_CENTER)
            .setVerticalAlignment(LayoutElementBuilders.VERTICAL_ALIGN_CENTER)
            .setModifiers(
                ModifiersBuilders.Modifiers.Builder()
                    .setClickable(cmdClickable(btn.command, page, pkg))
                    .setBackground(bgSolid(0x00000000))
                    .build()
            )
            .addContent(
                LayoutElementBuilders.Image.Builder()
                    .setResourceId(btn.resourceId)
                    .setWidth(DimensionBuilders.dp(ICON_DP))
                    .setHeight(DimensionBuilders.dp(ICON_DP))
                    .setColorFilter(
                        LayoutElementBuilders.ColorFilter.Builder()
                            .setTint(argb(0xFFFFFFFF))
                            .build()
                    )
                    .build()
            )
            .build()

    // ── Clickable: открыть приложение ────────────────

    private fun openClickable(page: Int, pkg: String) =
        ModifiersBuilders.Clickable.Builder()
            .setId("open")
            .setOnClick(launchAction(page, null, pkg))
            .build()

    private fun cmdClickable(cmd: String, page: Int, pkg: String) =
        ModifiersBuilders.Clickable.Builder()
            .setId("cmd_$cmd")
            .setOnClick(launchAction(page, cmd, pkg))
            .build()

    private fun launchAction(
        page: Int, cmd: String?, pkg: String
    ): ActionBuilders.LaunchAction {
        val ab = ActionBuilders.AndroidActivity.Builder()
            .setPackageName(pkg)
            .setClassName("$pkg.presentation.MainActivity")
            .addKeyToExtraMapping(
                "open_page",
                ActionBuilders.AndroidIntExtra.Builder().setValue(page).build()
            )
        if (cmd != null) {
            ab.addKeyToExtraMapping(
                "auto_cmd",
                ActionBuilders.AndroidStringExtra.Builder().setValue(cmd).build()
            )
        }
        return ActionBuilders.LaunchAction.Builder()
            .setAndroidActivity(ab.build()).build()
    }

    // ── Примитивы ────────────────────────────────────

    private fun titleText(t: String) =
        LayoutElementBuilders.Text.Builder()
            .setText(t)
            .setFontStyle(
                LayoutElementBuilders.FontStyle.Builder()
                    .setSize(DimensionBuilders.sp(TITLE_SP))
                    .setColor(argb(0xFFBBDEFB))
                    .build()
            ).build()

    private fun vSpacer(dp: Float) =
        LayoutElementBuilders.Spacer.Builder()
            .setHeight(DimensionBuilders.dp(dp)).build()

    private fun hSpacer(dp: Float) =
        LayoutElementBuilders.Spacer.Builder()
            .setWidth(DimensionBuilders.dp(dp)).build()

    private fun argb(c: Long) =
        ColorBuilders.ColorProp.Builder(c.toInt()).build()

    private fun bgSolid(color: Long, corner: Float = 0f): ModifiersBuilders.Background {
        val b = ModifiersBuilders.Background.Builder().setColor(argb(color))
        if (corner > 0f)
            b.setCorner(
                ModifiersBuilders.Corner.Builder()
                    .setRadius(DimensionBuilders.dp(corner)).build()
            )
        return b.build()
    }

    private fun padModifiers(h: Float) =
        ModifiersBuilders.Modifiers.Builder()
            .setPadding(
                ModifiersBuilders.Padding.Builder()
                    .setStart(DimensionBuilders.dp(h))
                    .setEnd(DimensionBuilders.dp(h))
                    .build()
            ).build()
}
