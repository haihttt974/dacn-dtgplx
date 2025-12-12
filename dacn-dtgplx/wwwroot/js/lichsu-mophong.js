(() => {
    const $ = id => document.getElementById(id);

    const video = $("video");
    const flagLayer = $("flagLayer");
    const scoreZoneLayer = $("scoreZoneLayer");
    const flagTable = $("flagTable");
    const txtFlagsTop = $("txtFlagsTop");
    const txtTimeTop = $("txtTimeTop");
    const txtTinhHuongTop = $("txtTinhHuongTop");
    const timelineProgress = $("timelineProgress");

    let idx = 0;
    const flags = [];

    const pad2 = n => String(n).padStart(2, "0");
    const fmt = s => `${pad2(s / 60 | 0)}:${pad2(s % 60 | 0)}`;

    function tinhDiem(t, th) {
        const s = th.scoreStartSec, e = th.scoreEndSec;
        if (t < s || t > e) return 0;
        const step = (e - s) / 5;
        return 5 - Math.min(4, Math.floor((t - s) / step));
    }

    function loadTinhHuong(i) {
        idx = i;
        const th = tinhHuongs[i];
        txtTinhHuongTop.textContent = `Tình huống ${i + 1}`;
        video.src = th.videoUrl;
        video.load();

        video.onloadedmetadata = () => {
            renderScoreZones(th);
            renderFlags();
        };
    }

    function renderScoreZones(th) {
        scoreZoneLayer.innerHTML = "";
        const step = (th.scoreEndSec - th.scoreStartSec) / 5;

        for (let i = 0; i < 5; i++) {
            const div = document.createElement("div");
            div.className = `mp-scoreZone score-${5 - i}`;
            div.style.left = (th.scoreStartSec + step * i) / video.duration * 100 + "%";
            div.style.width = step / video.duration * 100 + "%";
            scoreZoneLayer.appendChild(div);
        }
    }

    function renderFlags() {
        flagLayer.innerHTML = "";
        flags.filter(f => f.idx === idx && f.timeSec > 0)
            .forEach(f => {
                const pin = document.createElement("div");
                pin.className = "mp-flagPin";
                pin.textContent = "🚩";
                pin.style.left = (f.timeSec / video.duration * 100) + "%";
                flagLayer.appendChild(pin);
            });
    }

    function renderTable() {
        flagTable.innerHTML = flags.map((f, i) => `
            <tr onclick="window.__go(${f.idx}, ${f.timeSec})">
                <td>${i + 1}</td>
                <td>${fmt(f.timeSec)}</td>
                <td class="${f.diem > 0 ? "text-success" : "text-danger"}">
                    <strong>${f.diem}</strong>
                </td>
            </tr>
        `).join("");
    }

    // expose click
    window.__go = (i, t) => {
        loadTinhHuong(i);
        video.currentTime = t;
        video.pause();
    };

    // init flags
    reviewFlags.forEach(f => {
        const idx = tinhHuongs.findIndex(x => x.idThMp === f.idThMp);
        if (idx >= 0) {
            flags.push({
                idx,
                timeSec: f.timeSec,
                diem: tinhDiem(f.timeSec, tinhHuongs[idx])
            });
        }
    });

    txtFlagsTop.textContent = flags.length;
    renderTable();
    loadTinhHuong(0);

    $("btnShowResult").onclick = () =>
        new bootstrap.Modal($("resultModal")).show();

    video.ontimeupdate = () => {
        txtTimeTop.textContent = fmt(video.currentTime);
        timelineProgress.style.width =
            video.currentTime / video.duration * 100 + "%";
    };
})();
