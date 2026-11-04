const el = Object.fromEntries([
    "uploadForm", "pdfFile", "fileInfo", "analyzeButton", "statusText", "progressBar",
    "result", "resultSummary", "paragraphs", "tables", "uploadStep", "analyzeStep", "completeStep"
].map(id => [id, document.getElementById(id)]));

let busy = false;

function setStep(step, state) {
    el[`${step}Step`].dataset.state = state;
}

function resetSteps() {
    for (const step of ["upload", "analyze", "complete"]) setStep(step, "pending");
    el.progressBar.style.width = "0%";
}

function renderResult(data) {
    el.paragraphs.replaceChildren();
    el.tables.replaceChildren();
    el.resultSummary.textContent = `识别 ${data.pageCount} 页 · ${data.paragraphs.length} 个段落 · ${data.tables.length} 个表格`;

    const paragraphs = data.paragraphs.length ? data.paragraphs : (data.content ? [data.content] : []);
    for (const text of paragraphs) {
        const paragraph = document.createElement("p");
        paragraph.textContent = text;
        el.paragraphs.append(paragraph);
    }
    if (!paragraphs.length) {
        const empty = document.createElement("p");
        empty.textContent = "未识别到文字。";
        el.paragraphs.append(empty);
    }

    for (const [index, table] of data.tables.entries()) {
        const title = document.createElement("h4");
        title.textContent = `表格 ${index + 1}`;
        const grid = document.createElement("table");
        const body = document.createElement("tbody");
        const occupiedUntil = Array(table.columnCount).fill(0);
        for (let rowIndex = 0; rowIndex < table.rowCount; rowIndex++) {
            const row = document.createElement("tr");
            const cells = table.cells.filter(cell => cell.rowIndex === rowIndex)
                .sort((a, b) => a.columnIndex - b.columnIndex);
            let column = 0;
            function fillUntil(target) {
                while (column < target) {
                    if (occupiedUntil[column] <= rowIndex) row.append(document.createElement("td"));
                    column++;
                }
            }
            for (const cell of cells) {
                fillUntil(cell.columnIndex);
                const td = document.createElement("td");
                td.textContent = cell.content;
                td.rowSpan = cell.rowSpan;
                td.colSpan = cell.columnSpan;
                row.append(td);
                for (let i = 0; i < cell.columnSpan; i++)
                    occupiedUntil[cell.columnIndex + i] = rowIndex + cell.rowSpan;
                column = cell.columnIndex + cell.columnSpan;
            }
            fillUntil(table.columnCount);
            body.append(row);
        }
        grid.append(body);
        el.tables.append(title, grid);
    }
    el.result.hidden = false;
}

el.pdfFile.addEventListener("change", () => {
    const file = el.pdfFile.files[0];
    el.fileInfo.textContent = file ? `${file.name} · ${(file.size / 1024 / 1024).toFixed(2)} MB` : "请选择 PDF 文件";
});

el.uploadForm.addEventListener("submit", event => {
    event.preventDefault();
    if (busy) return;
    const file = el.pdfFile.files[0];
    resetSteps();
    el.result.hidden = true;
    if (!file || !/\.pdf$/i.test(file.name) || file.size < 5 || file.size > 10 * 1024 * 1024) {
        el.statusText.textContent = "请选择不超过 10 MB 的 PDF 文件。";
        return;
    }

    busy = true;
    el.analyzeButton.disabled = true;
    el.pdfFile.disabled = true;
    setStep("upload", "active");
    el.statusText.textContent = "正在上传 PDF…";

    const request = new XMLHttpRequest();
    request.open("POST", "/api/analyze");
    request.setRequestHeader("X-Demo-Request", "1");
    request.upload.addEventListener("progress", progress => {
        if (!progress.lengthComputable) return;
        const percent = Math.min(100, Math.round(progress.loaded / progress.total * 100));
        el.progressBar.style.width = `${percent}%`;
        el.statusText.textContent = `正在上传 PDF… ${percent}%`;
    });
    request.upload.addEventListener("load", () => {
        el.progressBar.style.width = "100%";
        setStep("upload", "done");
        setStep("analyze", "active");
        el.statusText.textContent = "上传完成，正在等待服务端和 Azure 解析…";
    });
    function finish() {
        busy = false;
        el.analyzeButton.disabled = false;
        el.pdfFile.disabled = false;
    }
    request.addEventListener("load", () => {
        let response;
        try {
            response = JSON.parse(request.responseText);
        } catch {
            el.statusText.textContent = "服务返回的内容无法读取，请稍后重试。";
            setStep("analyze", "error");
            finish();
            return;
        }
        if (request.status < 200 || request.status >= 300) {
            el.statusText.textContent = response.error || `解析失败（${request.status}）。`;
            setStep("analyze", "error");
            finish();
            return;
        }
        if (!response || !Number.isInteger(response.pageCount) || response.pageCount < 0
            || typeof response.content !== "string"
            || !Array.isArray(response.paragraphs) || response.paragraphs.some(text => typeof text !== "string")
            || !Array.isArray(response.tables) || response.tables.some(table =>
                !Number.isInteger(table.rowCount) || table.rowCount < 0
                || !Number.isInteger(table.columnCount) || table.columnCount < 0
                || !Array.isArray(table.cells) || table.cells.some(cell =>
                    !Number.isInteger(cell.rowIndex) || cell.rowIndex < 0 || cell.rowIndex >= table.rowCount
                    || !Number.isInteger(cell.columnIndex) || cell.columnIndex < 0 || cell.columnIndex >= table.columnCount
                    || !Number.isInteger(cell.rowSpan) || cell.rowSpan < 1 || cell.rowIndex + cell.rowSpan > table.rowCount
                    || !Number.isInteger(cell.columnSpan) || cell.columnSpan < 1 || cell.columnIndex + cell.columnSpan > table.columnCount
                    || typeof cell.content !== "string"))) {
            el.statusText.textContent = "服务返回的内容无法读取，请稍后重试。";
            setStep("analyze", "error");
            finish();
            return;
        }
        renderResult(response);
        setStep("upload", "done");
        setStep("analyze", "done");
        setStep("complete", "done");
        el.statusText.textContent = "解析完成，请核对识别结果。";
        finish();
    });
    request.addEventListener("error", () => {
        el.statusText.textContent = "无法连接服务，请确认 Demo 正在运行后重试。";
        setStep("analyze", "error");
        finish();
    });
    const form = new FormData();
    form.append("file", file);
    request.send(form);
});