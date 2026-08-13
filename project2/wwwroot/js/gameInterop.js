window.gameInterop = {
    dotNetRef: null,
    canvas: null,
    ctx: null,
    running: false,
    lastTimestamp: 0,
    topMargin: 0,
    bottomMargin: 0,
    sideMargin: 0,
    lastHeartDiameter: 40,
    lastState: null,
    skinIconRects: [],
    colorIconRects: [],

    init: function (dotNetRef, canvasId) {
        this.dotNetRef = dotNetRef;
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext('2d');

        this.resize();
        window.addEventListener('resize', () => this.resize());

        this.canvas.addEventListener('pointermove', (e) => this.onPointerMove(e));
        this.canvas.addEventListener('pointerdown', (e) => this.onPointerDown(e));

        this.running = true;
        requestAnimationFrame((t) => this.loop(t));
    },

    resize: function () {
        const rect = this.canvas.getBoundingClientRect();
        this.canvas.width = rect.width;
        this.canvas.height = rect.height;

        this.sideMargin = this.lastHeartDiameter * 1.5;
        const maxSideMargin = rect.width * 0.35;
        if (this.sideMargin > maxSideMargin) this.sideMargin = maxSideMargin;

        const playableWidth = Math.max(100, rect.width - this.sideMargin * 2);

        const goalWidth = playableWidth * 0.25;
        const goalDepth = goalWidth * 0.20;
        const topTextSpace = Math.max(70, rect.height * 0.08);
        const bottomTextSpace = Math.max(70, rect.height * 0.08);

        this.topMargin = topTextSpace + goalDepth;
        this.bottomMargin = bottomTextSpace;

        const playableHeight = Math.max(50, rect.height - this.topMargin - this.bottomMargin);

        this.dotNetRef.invokeMethodAsync('OnFieldResized', playableWidth, playableHeight);
    },

    onPointerMove: function (e) {
        const rect = this.canvas.getBoundingClientRect();
        const x = e.clientX - rect.left - this.sideMargin;
        this.dotNetRef.invokeMethodAsync('OnPointerMove', x);
    },

    onPointerDown: function (e) {
        const rect = this.canvas.getBoundingClientRect();
        const x = e.clientX - rect.left;
        const y = e.clientY - rect.top;

        if (this.lastState && this.lastState.isWaiting) {
            for (const icon of this.skinIconRects) {
                if (x >= icon.x && x <= icon.x + icon.w && y >= icon.y && y <= icon.y + icon.h) {
                    this.dotNetRef.invokeMethodAsync('OnSkinSelected', icon.skin);
                    return;
                }
            }
            for (const icon of this.colorIconRects) {
                if (x >= icon.x && x <= icon.x + icon.w && y >= icon.y && y <= icon.y + icon.h) {
                    this.dotNetRef.invokeMethodAsync('OnFieldColorSelected', icon.color);
                    return;
                }
            }
        }

        this.dotNetRef.invokeMethodAsync('OnPointerDown');
    },

    loop: function (timestamp) {
        if (!this.running) return;

        const dt = this.lastTimestamp ? (timestamp - this.lastTimestamp) / 1000 : 0;
        this.lastTimestamp = timestamp;
        const clampedDt = Math.min(dt, 0.05);

        this.dotNetRef.invokeMethodAsync('OnTick', clampedDt)
            .then((state) => this.render(state));

        requestAnimationFrame((t) => this.loop(t));
    },

    render: function (state) {
        if (!state || state.paddleWidth === undefined) return;
        this.lastState = state;

        const ctx = this.ctx;
        const w = state.fieldWidth;
        const fieldH = state.fieldHeight;

        ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        this.drawWoodBackground(ctx);

        // Левая панель выбора мяча, правая — выбора цвета поля
        this.drawSkinPanel(ctx, state);
        this.drawColorPanel(ctx, state);

        ctx.save();
        ctx.translate(this.sideMargin, this.topMargin);

        // Заливка игрового поля выбранным цветом (только внутренняя область, не панели)
        ctx.fillStyle = this.fieldColors[state.fieldColor] ?? this.fieldColors[0];
        ctx.fillRect(0, 0, w, fieldH);

        const borderWidth = Math.max(4, Math.min(w, fieldH) * 0.012);

        ctx.strokeStyle = '#1e293b';
        ctx.lineWidth = borderWidth;
        ctx.beginPath();
        ctx.moveTo(borderWidth / 2, 0); ctx.lineTo(borderWidth / 2, fieldH);
        ctx.stroke();

        ctx.beginPath();
        ctx.moveTo(w - borderWidth / 2, 0); ctx.lineTo(w - borderWidth / 2, fieldH);
        ctx.stroke();

        ctx.beginPath();
        ctx.moveTo(0, fieldH - borderWidth / 2); ctx.lineTo(w, fieldH - borderWidth / 2);
        ctx.stroke();

        ctx.beginPath();
        ctx.moveTo(0, borderWidth / 2); ctx.lineTo(state.goalLeft, borderWidth / 2);
        ctx.moveTo(state.goalRight, borderWidth / 2); ctx.lineTo(w, borderWidth / 2);
        ctx.stroke();

        const goalDepth = (state.goalRight - state.goalLeft) * 0.20;
        this.drawGoalNet(ctx, state.goalLeft, state.goalRight, -goalDepth, 0, borderWidth);

        ctx.strokeStyle = '#22c55e';
        ctx.lineWidth = borderWidth * 1.2;
        ctx.beginPath();
        ctx.moveTo(state.goalLeft, 0);
        ctx.lineTo(state.goalRight, 0);
        ctx.stroke();

        ctx.fillStyle = '#2563eb';
        ctx.fillRect(state.paddleLeft, state.paddleY, state.paddleWidth, state.paddleHeight);

        this.drawBall(ctx, state.skin, state.heartX, state.heartY, state.heartRadius);

        ctx.restore();

        this.lastHeartDiameter = state.heartRadius * 1.6;
    },

    drawWoodBackground: function (ctx) {
        const w = this.canvas.width;
        const h = this.canvas.height;

        // базовый цвет дерева
        ctx.fillStyle = '#d8b98a';
        ctx.fillRect(0, 0, w, h);

        // волокна древесины — горизонтальные волнистые линии разной прозрачности
        ctx.save();
        const lineCount = Math.max(12, Math.floor(h / 18));

        for (let i = 0; i < lineCount; i++) {
            const baseY = (h / lineCount) * i + (h / lineCount) / 2;
            const amplitude = 4 + (i % 3) * 2;
            const frequency = 0.01 + (i % 4) * 0.003;
            const shade = i % 2 === 0 ? 'rgba(120, 84, 44, 0.18)' : 'rgba(160, 120, 70, 0.15)';

            ctx.strokeStyle = shade;
            ctx.lineWidth = 1.5;
            ctx.beginPath();

            for (let x = 0; x <= w; x += 4) {
                const y = baseY + Math.sin(x * frequency + i) * amplitude;
                if (x === 0) ctx.moveTo(x, y);
                else ctx.lineTo(x, y);
            }
            ctx.stroke();
        }

        // редкие "сучки"
        const knotCount = Math.max(2, Math.floor(w / 500));
        for (let k = 0; k < knotCount; k++) {
            const kx = (w / (knotCount + 1)) * (k + 1) + (k % 2 === 0 ? 30 : -30);
            const ky = h * (0.2 + (k % 3) * 0.3);
            const kr = 10 + (k % 3) * 4;

            const gradient = ctx.createRadialGradient(kx, ky, 1, kx, ky, kr);
            gradient.addColorStop(0, 'rgba(90, 60, 30, 0.5)');
            gradient.addColorStop(1, 'rgba(90, 60, 30, 0)');
            ctx.fillStyle = gradient;
            ctx.beginPath();
            ctx.arc(kx, ky, kr, 0, Math.PI * 2);
            ctx.fill();
        }

        ctx.restore();
    },

    // ---- Панель выбора мяча ----

    skinColors: {
        0: '#e11d48', // красное сердце
        1: '#38bdf8', // голубое сердце
        2: '#16a34a', // зелёное сердце
        3: '#111827', // чёрный квадрат
        4: '#7c3aed'  // фиолетовый ромб
    },

    fieldColors: {
        0: '#ffffff',
        1: '#fff7cc',
        2: '#d9e8d3',
        3: '#e8d5bd',
        4: '#f9d7e3'
    },

    drawSkinPanel: function (ctx, state) {
        this.skinIconRects = [];

        const iconSize = Math.min(this.sideMargin * 0.55, 56);
        const gap = iconSize * 0.5;
        const count = 5;
        const totalHeight = count * iconSize + (count - 1) * gap;
        const startY = (this.canvas.height - totalHeight) / 2;
        const centerX = this.sideMargin / 2;

        for (let i = 0; i < count; i++) {
            const cy = startY + i * (iconSize + gap) + iconSize / 2;
            const rectX = centerX - iconSize / 2;
            const rectY = cy - iconSize / 2;

            this.skinIconRects.push({ x: rectX, y: rectY, w: iconSize, h: iconSize, skin: i });

            // подсветка выбранной иконки
            if (state.skin === i) {
                ctx.fillStyle = 'rgba(37, 99, 235, 0.15)';
                ctx.beginPath();
                ctx.roundRect(rectX - 6, rectY - 6, iconSize + 12, iconSize + 12, 8);
                ctx.fill();
            }

            const color = this.skinColors[i];
            const radius = iconSize * 0.35;

            switch (i) {
                case 0:
                case 1:
                case 2:
                    this.drawHeartShape(ctx, centerX, cy, radius, color, '#1e293b');
                    break;
                case 3:
                    this.drawSquareShape(ctx, centerX, cy, radius, color, '#ffffff');
                    break;
                case 4:
                    this.drawDiamondShape(ctx, centerX, cy, radius, color, '#1e293b');
                    break;
            }
        }
    },

    drawColorPanel: function (ctx, state) {
        this.colorIconRects = [];

        const iconSize = Math.min(this.sideMargin * 0.55, 56);
        const gap = iconSize * 0.5;
        const count = 5;
        const totalHeight = count * iconSize + (count - 1) * gap;
        const startY = (this.canvas.height - totalHeight) / 2;
        const centerX = this.canvas.width - this.sideMargin / 2;

        for (let i = 0; i < count; i++) {
            const cy = startY + i * (iconSize + gap) + iconSize / 2;
            const rectX = centerX - iconSize / 2;
            const rectY = cy - iconSize / 2;

            this.colorIconRects.push({ x: rectX, y: rectY, w: iconSize, h: iconSize, color: i });

            if (state.fieldColor === i) {
                ctx.fillStyle = 'rgba(37, 99, 235, 0.15)';
                ctx.beginPath();
                ctx.roundRect(rectX - 6, rectY - 6, iconSize + 12, iconSize + 12, 8);
                ctx.fill();
            }

            const half = iconSize * 0.35;
            ctx.save();
            ctx.fillStyle = this.fieldColors[i];
            ctx.fillRect(centerX - half, cy - half, half * 2, half * 2);
            ctx.lineWidth = Math.max(1, half * 0.12);
            ctx.strokeStyle = '#1e293b';
            ctx.strokeRect(centerX - half, cy - half, half * 2, half * 2);
            ctx.restore();
        }
    },

    // ---- Отрисовка самого летающего мяча по текущему скину ----

    drawBall: function (ctx, skin, cx, cy, radius) {
        const color = this.skinColors[skin] ?? this.skinColors[0];

        switch (skin) {
            case 3:
                this.drawSquareShape(ctx, cx, cy, radius, color, '#ffffff');
                break;
            case 4:
                this.drawDiamondShape(ctx, cx, cy, radius, color, '#1e293b');
                break;
            default:
                this.drawHeartShape(ctx, cx, cy, radius, color, '#1e293b');
                break;
        }
    },

    // ---- Формы ----

    drawHeartShape: function (ctx, cx, cy, radius, fillColor, strokeColor) {
        const s = radius * 1.6;
        const topCurveHeight = s * 0.3;
        const x = cx;
        const y = cy - s * 0.4;

        ctx.save();
        ctx.beginPath();
        ctx.moveTo(x, y + topCurveHeight);

        ctx.bezierCurveTo(x, y, x - s / 2, y, x - s / 2, y + topCurveHeight);
        ctx.bezierCurveTo(x - s / 2, y + (s + topCurveHeight) / 2, x, y + (s + topCurveHeight) / 2, x, y + s);
        ctx.bezierCurveTo(x, y + (s + topCurveHeight) / 2, x + s / 2, y + (s + topCurveHeight) / 2, x + s / 2, y + topCurveHeight);
        ctx.bezierCurveTo(x + s / 2, y, x, y, x, y + topCurveHeight);

        ctx.closePath();
        ctx.fillStyle = fillColor;
        ctx.fill();
        ctx.lineWidth = Math.max(1, radius * 0.12);
        ctx.strokeStyle = strokeColor;
        ctx.stroke();
        ctx.restore();
    },

    drawSquareShape: function (ctx, cx, cy, radius, fillColor, strokeColor) {
        const half = radius * 0.9;
        ctx.save();
        ctx.fillStyle = fillColor;
        ctx.fillRect(cx - half, cy - half, half * 2, half * 2);
        ctx.lineWidth = Math.max(1, radius * 0.12);
        ctx.strokeStyle = strokeColor;
        ctx.strokeRect(cx - half, cy - half, half * 2, half * 2);
        ctx.restore();
    },

    drawDiamondShape: function (ctx, cx, cy, radius, fillColor, strokeColor) {
        ctx.save();
        ctx.beginPath();
        ctx.moveTo(cx, cy - radius);
        ctx.lineTo(cx + radius, cy);
        ctx.lineTo(cx, cy + radius);
        ctx.lineTo(cx - radius, cy);
        ctx.closePath();
        ctx.fillStyle = fillColor;
        ctx.fill();
        ctx.lineWidth = Math.max(1, radius * 0.12);
        ctx.strokeStyle = strokeColor;
        ctx.stroke();
        ctx.restore();
    },

    drawGoalNet: function (ctx, xLeft, xRight, yTop, yBottom, borderWidth) {
        const width = xRight - xLeft;
        const height = yBottom - yTop;

        ctx.fillStyle = '#f1f5f9';
        ctx.fillRect(xLeft, yTop, width, height);

        ctx.strokeStyle = '#94a3b8';
        ctx.lineWidth = Math.max(1, borderWidth * 0.25);

        const cols = 12;
        const rows = 6;

        for (let i = 1; i < cols; i++) {
            const x = xLeft + (width / cols) * i;
            ctx.beginPath();
            ctx.moveTo(x, yTop);
            ctx.lineTo(x, yBottom);
            ctx.stroke();
        }

        for (let j = 1; j < rows; j++) {
            const y = yTop + (height / rows) * j;
            ctx.beginPath();
            ctx.moveTo(xLeft, y);
            ctx.lineTo(xRight, y);
            ctx.stroke();
        }

        ctx.strokeStyle = '#1e293b';
        ctx.lineWidth = borderWidth;
        ctx.strokeRect(xLeft, yTop, width, height);
    }
};