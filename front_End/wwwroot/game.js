// BSP Stealth Game - Canvas Oyun Motoru
// Düşman hareketi: backend A* + BSP (back_End)
// Düşman kararı: a_i servisi (asenkron)
// FOV: backend BSP raycasting

let gameState = null;
const BACKEND = window.location.hostname === 'localhost'
    ? 'http://localhost:5050'
    : 'http://back_end:8080';

window.initCanvas = function () { };

window.loadMap = function (map) {
    const canvas = document.getElementById('gameCanvas');
    if (!canvas) return;

    gameState = {
        map: map,
        player: { x: map.playerStart.x, y: map.playerStart.y },
        enemies: map.enemies.map((e, i) => ({
            id: i, x: e.x, y: e.y,
            angle: i * 90,
            state: 'patrol',
            fovPolygon: []
        })),
        keys: {},
        animId: null,
        frameCount: 0,
        caught: false,
        reached: false
    };

    draw(canvas.getContext('2d'));
};

window.startGame = function () {
    if (!gameState) return;
    const canvas = document.getElementById('gameCanvas');
    const ctx = canvas.getContext('2d');
    if (gameState.animId) cancelAnimationFrame(gameState.animId);

    window.onkeydown = e => { gameState.keys[e.key] = true; };
    window.onkeyup = e => { gameState.keys[e.key] = false; };

    function loop() {
        const gs = gameState;
        gs.frameCount++;

        // Oyuncu hareketini güncelle
        movePlayer(gs);

        // Her 6 frame'de backend'e güncelleme gönder
        // Backend: A* ile düşman hareketi + BSP görüş testi
        if (gs.frameCount % 6 === 0) {
            sendUpdate(gs);
        }

        // Her 8 frame'de FOV çek
        if (gs.frameCount % 8 === 0) {
            fetchFOV(gs);
        }

        draw(ctx);

        if (!gs.caught && !gs.reached)
            gs.animId = requestAnimationFrame(loop);
    }
    loop();
};

function movePlayer(gs) {
    const p = gs.player;
    const speed = 4;
    const k = gs.keys;

    let newX = p.x;
    let newY = p.y;

    if (k['w'] || k['W'] || k['ArrowUp']) newY -= speed;
    if (k['s'] || k['S'] || k['ArrowDown']) newY += speed;
    if (k['a'] || k['A'] || k['ArrowLeft']) newX -= speed;
    if (k['d'] || k['D'] || k['ArrowRight']) newX += speed;

    const maxX = gs.map.gridWidth * 40;
    const maxY = gs.map.gridHeight * 40;
    newX = Math.max(10, Math.min(maxX - 10, newX));
    newY = Math.max(10, Math.min(maxY - 10, newY));

    // Duvar çarpışma kontrolü — duvar segmentlerine mesafe kontrolü
    let blocked = false;
    for (const w of gs.map.walls) {
        if (distToSegment(newX, newY, w.x1, w.y1, w.x2, w.y2) < 8) {
            blocked = true;
            break;
        }
    }

    if (!blocked) {
        p.x = newX;
        p.y = newY;
    }
}

function distToSegment(px, py, x1, y1, x2, y2) {
    const dx = x2 - x1, dy = y2 - y1;
    const lenSq = dx * dx + dy * dy;
    if (lenSq === 0) return Math.sqrt((px - x1) ** 2 + (py - y1) ** 2);
    let t = ((px - x1) * dx + (py - y1) * dy) / lenSq;
    t = Math.max(0, Math.min(1, t));
    return Math.sqrt((px - (x1 + t * dx)) ** 2 + (py - (y1 + t * dy)) ** 2);
}

// Backend'e oyuncu pozisyonu gönder, düşman güncellemesi al
function sendUpdate(gs) {
    fetch(`${BACKEND}/api/game/update`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ playerX: gs.player.x, playerY: gs.player.y })
    })
        .then(r => r.json())
        .then(data => {
            // Backend'den gelen düşman pozisyonlarını güncelle
            data.enemies.forEach(be => {
                const e = gs.enemies.find(e => e.id === be.id);
                if (e) {
                    e.x = be.x;
                    e.y = be.y;
                    e.angle = be.angle;
                    e.state = be.state;
                }
            });

            // Backend onaylı oyuncu pozisyonunu kullan
            if (data.playerX !== undefined) {
                gs.player.x = data.playerX;
                gs.player.y = data.playerY;
            }

            gs.caught = data.caught;
            gs.reached = data.reached;
        })
        .catch(() => { });
}
function fetchFOV(gs) {
    gs.enemies.forEach((e, i) => {
        fetch(`http://localhost:5051/api/ai/fov?ex=${Math.round(e.x)}&ey=${Math.round(e.y)}&angle=${Math.round(e.angle)}&fov=90&rays=60&dist=180`)
            .then(r => r.json())
            .then(data => {
                if (data && data.fovPoints)
                    gs.enemies[i].fovPolygon = data.fovPoints;
            })
            .catch(() => { });
    });
}

function draw(ctx) {
    if (!gameState) return;
    const gs = gameState;
    const canvas = ctx.canvas;
    const map = gs.map;

    const mapW = map.gridWidth * 40;
    const mapH = map.gridHeight * 40;
    const sx = canvas.width / mapW;
    const sy = canvas.height / mapH;

    ctx.fillStyle = '#111';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.save();
    ctx.scale(sx, sy);

    // Duvarlar
    ctx.strokeStyle = '#4488ff';
    ctx.lineWidth = 1.5 / sx;
    map.walls.forEach(w => {
        ctx.beginPath();
        ctx.moveTo(w.x1, w.y1);
        ctx.lineTo(w.x2, w.y2);
        ctx.stroke();
    });

    // Hedef
    ctx.fillStyle = '#ffff00';
    ctx.beginPath();
    ctx.arc(map.goal.x, map.goal.y, 10, 0, Math.PI * 2);
    ctx.fill();
    ctx.fillStyle = '#000';
    ctx.font = `${13 / sx}px monospace`;
    ctx.fillText('HEDEF', map.goal.x - 22, map.goal.y - 13);

    // Düşmanlar
    gs.enemies.forEach(e => {
        const r = e.angle * Math.PI / 180;
        const isChasing = e.state === 'chase';

        // FOV — BSP raycasting poligonu (backend'den)
        if (e.fovPolygon && e.fovPolygon.length > 1) {
            ctx.fillStyle = isChasing
                ? 'rgba(255, 0, 0, 0.35)'
                : 'rgba(255, 100, 50, 0.2)';
            ctx.beginPath();
            ctx.moveTo(e.x, e.y);
            e.fovPolygon.forEach(pt => ctx.lineTo(pt.x, pt.y));
            ctx.closePath();
            ctx.fill();
        } else {
            // BSP cevabı gelmeden basit koni
            const fovHalf = 45 * Math.PI / 180;
            ctx.fillStyle = 'rgba(255, 50, 50, 0.15)';
            ctx.beginPath();
            ctx.moveTo(e.x, e.y);
            for (let i = -15; i <= 15; i++) {
                const a = r + (i / 15) * fovHalf;
                ctx.lineTo(e.x + Math.cos(a) * 160, e.y + Math.sin(a) * 160);
            }
            ctx.closePath();
            ctx.fill();
        }

        // Düşman rengi — chase ise turuncu, patrol ise kırmızı
        ctx.fillStyle = isChasing ? '#ff8800' : '#ff3333';
        ctx.beginPath();
        ctx.arc(e.x, e.y, 12, 0, Math.PI * 2);
        ctx.fill();

        // Yön oku
        ctx.strokeStyle = isChasing ? '#ffaa00' : '#ff0000';
        ctx.lineWidth = 2 / sx;
        ctx.beginPath();
        ctx.moveTo(e.x, e.y);
        ctx.lineTo(e.x + Math.cos(r) * 25, e.y + Math.sin(r) * 25);
        ctx.stroke();

        // Durum etiketi
        if (isChasing) {
            ctx.fillStyle = '#ff8800';
            ctx.font = `${10 / sx}px monospace`;
            ctx.fillText('TAKİP!', e.x - 15, e.y - 16);
        }
    });

    // Oyuncu
    const p = gs.player;
    ctx.fillStyle = '#00ff88';
    ctx.beginPath();
    ctx.arc(p.x, p.y, 10, 0, Math.PI * 2);
    ctx.fill();

    ctx.restore();

    // HUD
    ctx.fillStyle = '#00ff88';
    ctx.font = '12px monospace';
    ctx.fillText(`Oyuncu: (${Math.round(p.x)}, ${Math.round(p.y)})`, 10, 18);
    ctx.fillText(`Düşman: ${gs.enemies.length}`, 10, 34);
    ctx.fillStyle = '#aaaaaa';
    ctx.fillText('WASD / Ok tuşları ile hareket', 10, canvas.height - 10);

    // Oyun bitti ekranı
    if (gs.caught) {
        ctx.fillStyle = 'rgba(255,0,0,0.5)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.fillStyle = '#fff';
        ctx.font = 'bold 28px monospace';
        ctx.fillText('YAKALANDIN!', canvas.width / 2 - 100, canvas.height / 2);
        ctx.font = '14px monospace';
        ctx.fillText('Yeni harita üretmek için butona bas', canvas.width / 2 - 130, canvas.height / 2 + 30);
    }

    if (gs.reached) {
        ctx.fillStyle = 'rgba(0,200,0,0.5)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.fillStyle = '#fff';
        ctx.font = 'bold 28px monospace';
        ctx.fillText('HEDEFE ULAŞTIN!', canvas.width / 2 - 120, canvas.height / 2);
    }
}