(() => {
    /* ===================== ELEMENTS ===================== */
    const $ = id => document.getElementById(id);

    const video = $("video");
    const txtTimeTop = $("txtTimeTop");
    const txtFlagsTop = $("txtFlagsTop");
    const txtTinhHuongTop = $("txtTinhHuongTop");

    const timelineProgress = $("timelineProgress");
    const flagLayer = $("flagLayer");
    const scoreZoneLayer = $("scoreZoneLayer");
    const btnNextBottom = $("btnNextBottom");

    const btnMain = $("btnMain");
    const btnHint = $("btnHint");
    const btnNext = $("btnNext");

    const flagTable = $("flagTable");
    const stopModalEl = $("stopModal");
    const resultModalEl = $("resultModal");

    /* ===================== STATE ===================== */
    let examStarted = false;
    let submitted = false;
    let isPlaying = false;
    let idx = 0;
    let showAnswer = false;

    // flags: { idThMp, timeSec, diem, idx }
    const flags = [];

    if (!Array.isArray(tinhHuongs) || tinhHuongs.length === 0) {
        console.error("❌ tinhHuongs rỗng");
        return;
    }

    /* ===================== HELPERS ===================== */
    const pad2 = n => String(n).padStart(2, "0");
    const fmtMMSS = s =>
        `${pad2(Math.floor(s / 60))}:${pad2(Math.floor(s % 60))}`;

    function setFlagsCount() {
        txtFlagsTop.textContent = flags.length;
    }

    function clearTimeline() {
        timelineProgress.style.width = "0%";
        if (!showAnswer) {
            flagLayer.innerHTML = "";
            scoreZoneLayer.innerHTML = "";
        }
    }

    /* ===================== FLAGS ===================== */
    function addPin(timeSec) {
        if (!video.duration || video.duration <= 0) return;

        const percent = (timeSec / video.duration) * 100;

        const pin = document.createElement("div");
        pin.className = "mp-flagPin";
        pin.textContent = "🚩";
        pin.style.left = percent + "%";

        flagLayer.appendChild(pin);
    }

    function renderFlagsForCurrentTinhHuong() {
        flagLayer.innerHTML = "";

        if (!video.duration || video.duration <= 0) return;

        flags
            .filter(f => f.idx === idx && f.timeSec > 0)
            .forEach(f => addPin(f.timeSec));
    }

    /* ===================== SCORING ===================== */
    function tinhDiem(timeSec, th) {
        const start = Number(th.scoreStartSec);
        const end = Number(th.scoreEndSec);

        if (timeSec < start || timeSec > end) return 0;

        const step = (end - start) / 5;
        const index = Math.min(4, Math.max(0, Math.floor((timeSec - start) / step)));
        return 5 - index;
    }

    /* ===================== TABLE ===================== */
    function renderTable() {
        if (flags.length === 0) {
            flagTable.innerHTML =
                `<tr><td colspan="3" class="text-center text-muted">Chưa có cờ nào</td></tr>`;
            return;
        }

        flagTable.innerHTML = flags
            .sort((a, b) => a.idx - b.idx)
            .map((f, i) => `
                <tr class="flag-row" data-idx="${f.idx}" data-time="${f.timeSec}">
                    <td>${i + 1}</td>
                    <td>${fmtMMSS(f.timeSec)}</td>
                    <td class="${f.diem > 0 ? "text-success" : "text-danger"}">
                        <strong>${f.diem}</strong>
                    </td>
                </tr>
            `).join("");

        document.querySelectorAll(".flag-row").forEach(row => {
            row.onclick = async () => {
                const i = Number(row.dataset.idx);
                const t = Number(row.dataset.time);
                await loadTinhHuong(i, false);
                video.currentTime = t;
                video.pause();
            };
        });
    }

    /* ===================== PLACE FLAG ===================== */
    function placeFlag() {
        if (!examStarted || submitted) return;

        const th = tinhHuongs[idx];
        if (flags.some(f => f.idThMp === th.idThMp)) return;

        const time = video.currentTime;
        const diem = tinhDiem(time, th);

        flags.push({
            idThMp: th.idThMp,
            timeSec: time,
            diem,
            idx
        });

        setFlagsCount();
        renderTable();
        renderFlagsForCurrentTinhHuong(); // ✅ HIỆN NGAY
        //btnNext.style.display = "block";
        btnNextBottom.style.display = "inline-flex";

    }

    function autoZeroScoreIfMissing() {
        const th = tinhHuongs[idx];
        if (flags.some(f => f.idThMp === th.idThMp)) return;

        flags.push({
            idThMp: th.idThMp,
            timeSec: 0,
            diem: 0,
            idx
        });

        setFlagsCount();
        renderTable();
    }

    /* ===================== LOAD VIDEO ===================== */
    async function loadTinhHuong(i, autoplay) {
        idx = i;
        const th = tinhHuongs[idx];

        clearTimeline();
        //btnNext.style.display = "none";
        btnNextBottom.style.display = "none";

        txtTinhHuongTop.textContent = `Tình huống ${idx + 1}`;

        btnHint.style.display =
            (submitted && th.hintImageUrl) ? "inline-flex" : "none";

        video.pause();
        video.src = th.videoUrl;
        video.load();

        await new Promise(res =>
            video.addEventListener("loadedmetadata", res, { once: true })
        );

        if (showAnswer) {
            renderScoreZones(th);
            renderFlagsForCurrentTinhHuong();
        }

        if (autoplay) video.play();
    }

    /* ===================== EVENTS ===================== */
    window.addEventListener("keydown", e => {
        if (e.code === "Space") {
            e.preventDefault();
            placeFlag();
        }
    }, true);

    video.addEventListener("timeupdate", () => {
        txtTimeTop.textContent = fmtMMSS(video.currentTime);
        if (video.duration) {
            timelineProgress.style.width =
                (video.currentTime / video.duration * 100) + "%";
        }
    });

    video.addEventListener("ended", async () => {
        autoZeroScoreIfMissing();

        if (idx < tinhHuongs.length - 1) {
            await loadTinhHuong(idx + 1, true);
        } else {
            submitExam();
        }
    });

    /* ===================== START / STOP ===================== */
    btnMain.onclick = () => {
        // CHƯA BẮT ĐẦU
        if (!examStarted) {
            examStarted = true;
            isPlaying = true;

            btnMain.innerHTML = "⏹ Dừng";
            btnMain.classList.replace("btn-primary", "btn-danger");

            loadTinhHuong(0, true);
            return;
        }

        // ĐANG CHẠY → DỪNG
        if (isPlaying) {
            video.pause();
            isPlaying = false;
            new bootstrap.Modal(stopModalEl).show();
        }
    };

    // Khi đóng modal DỪNG → tiếp tục thi
    stopModalEl.addEventListener("hidden.bs.modal", () => {
        if (examStarted && !submitted) {
            video.play();
            isPlaying = true;
        }
    });

    /* ===================== HINT ===================== */
    const hintModalEl = document.getElementById("hintModal");
    const hintImageEl = document.getElementById("hintImage");

    btnHint.onclick = () => {
        const th = tinhHuongs[idx];
        if (!th?.hintImageUrl) return;

        hintImageEl.src = th.hintImageUrl;
        new bootstrap.Modal(hintModalEl).show();
    };

    /* ===================== SUBMIT ===================== */
    // ===================== SUBMIT =====================
    async function submitExam() {
        if (submitted) return;

        submitted = true;
        showAnswer = true;
        isPlaying = false;
        video.pause();

        // 🔑 PHÂN BIỆT ĐỀ THƯỜNG / ĐỀ NGẪU NHIÊN
        const isRandomExam = !idBoDe || idBoDe <= 0;

        const url = isRandomExam
            ? "/ThiMoPhong/LuuKetQuaNgauNhien"
            : "/ThiMoPhong/LuuKetQua";

        const payload = isRandomExam
            ? {
                selectedThIds: tinhHuongs.map(x => x.idThMp),
                flags: flags.map(f => ({
                    idThMp: f.idThMp,
                    timeSec: f.timeSec
                }))
            }
            : {
                idBoDe,
                flags: flags.map(f => ({
                    idThMp: f.idThMp,
                    timeSec: f.timeSec
                }))
            };

        const res = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        const data = await res.json();

        $("modalTongDiem").textContent = data.tongDiem;

        const modalKetQua = $("modalKetQua");
        modalKetQua.textContent = data.tongDiem >= 35 ? "✅ ĐẬU" : "❌ RỚT";
        modalKetQua.className = data.tongDiem >= 35
            ? "fs-4 fw-bold text-success"
            : "fs-4 fw-bold text-danger";

        timelineProgress.style.width = "100%";
        renderScoreZones(tinhHuongs[idx]);
        renderFlagsForCurrentTinhHuong();

        new bootstrap.Modal(resultModalEl, {
            backdrop: "static",
            keyboard: false
        }).show();
    }

    function renderScoreZones(th) {
        scoreZoneLayer.innerHTML = "";
        if (!video.duration) return;

        const start = th.scoreStartSec;
        const end = th.scoreEndSec;
        const step = (end - start) / 5;

        for (let i = 0; i < 5; i++) {
            const segStart = start + step * i;
            const segEnd = segStart + step;

            const div = document.createElement("div");
            div.className = `mp-scoreZone score-${5 - i}`;
            div.style.left = (segStart / video.duration) * 100 + "%";
            div.style.width = ((segEnd - segStart) / video.duration) * 100 + "%";

            scoreZoneLayer.appendChild(div);
        }
    }

    resultModalEl.addEventListener("hidden.bs.modal", () => {
        loadTinhHuong(idx, false);
    });

    btnNext.onclick = async () => {
        video.pause();

        // đảm bảo có flag (phòng trường hợp)
        autoZeroScoreIfMissing();

        if (idx < tinhHuongs.length - 1) {
            await loadTinhHuong(idx + 1, true);
        } else {
            submitExam();
        }
    };
    btnNextBottom.onclick = btnNext.onclick;

    /* ===================== INIT ===================== */
    setFlagsCount();
    renderTable();
})();
