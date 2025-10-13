const populationCtx = document.getElementById('populationChart');
const compositionCtx = document.getElementById('compositionChart');
const eventsCtx = document.getElementById('eventsChart');
const eventLogContainer = document.getElementById('event-log');

const connectionStatusEl = document.getElementById('connection-status');
const runStatusEl = document.getElementById('run-status');
const startYearEl = document.getElementById('start-year');
const currentYearEl = document.getElementById('current-year');
const totalPopulationEl = document.getElementById('total-population');
const aliveHumansEl = document.getElementById('alive-humans');
const aliveAnimalsEl = document.getElementById('alive-animals');

const startBtn = document.getElementById('start-btn');
const pauseBtn = document.getElementById('pause-btn');
const stepBtn = document.getElementById('step-btn');
const resetBtn = document.getElementById('reset-btn');

const numberFormatter = new Intl.NumberFormat('tr-TR');

let history = [];
let snapshotState = {
    startYear: null,
    currentYear: null,
    isRunning: false,
};
let isConnected = false;

let populationChart;
let compositionChart;
let eventsChart;
let connection;

function resolveHubUrl() {
    const params = new URLSearchParams(window.location.search);
    const explicit = params.get('hub');
    if (explicit) {
        return explicit;
    }

    const protocol = window.location.protocol.startsWith('https') ? 'https:' : 'http:';
    const host = window.location.hostname || 'localhost';
    const port = params.get('hubPort') ?? '5000';
    return `${protocol}//${host}:${port}/simulationHub`;
}

function initCharts() {
    populationChart = new Chart(populationCtx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [
                {
                    label: 'Toplam Nüfus',
                    data: [],
                    fill: true,
                    tension: 0.35,
                    borderColor: '#38bdf8',
                    backgroundColor: 'rgba(56, 189, 248, 0.2)',
                    pointRadius: 4,
                },
            ],
        },
        options: {
            animation: false,
            scales: {
                y: {
                    beginAtZero: true,
                },
            },
        },
    });

    compositionChart = new Chart(compositionCtx, {
        type: 'bar',
        data: {
            labels: [],
            datasets: [
                {
                    label: 'İnsan',
                    data: [],
                    backgroundColor: 'rgba(96, 165, 250, 0.7)',
                    stack: 'stack',
                },
                {
                    label: 'Hayvan',
                    data: [],
                    backgroundColor: 'rgba(74, 222, 128, 0.7)',
                    stack: 'stack',
                },
            ],
        },
        options: {
            animation: false,
            responsive: true,
            scales: {
                x: { stacked: true },
                y: { stacked: true, beginAtZero: true },
            },
        },
    });

    eventsChart = new Chart(eventsCtx, {
        type: 'radar',
        data: {
            labels: [],
            datasets: [],
        },
        options: {
            animation: false,
            responsive: true,
            elements: {
                line: {
                    borderWidth: 2,
                },
            },
            scales: {
                r: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0,
                    },
                },
            },
        },
    });
}

function setConnectionStatus(status, text) {
    connectionStatusEl.textContent = text;
    connectionStatusEl.classList.remove('online', 'offline', 'reconnecting', 'error');
    connectionStatusEl.classList.add(status);
}

function updateRunStatus() {
    runStatusEl.classList.remove('running', 'idle', 'completed');

    if (!history.length) {
        runStatusEl.textContent = 'Hazır';
        runStatusEl.classList.add('idle');
        return;
    }

    if (snapshotState.isRunning) {
        runStatusEl.textContent = 'Çalışıyor';
        runStatusEl.classList.add('running');
        return;
    }

    const last = history[history.length - 1];
    if (last.totalPopulation === 0) {
        runStatusEl.textContent = 'Tamamlandı';
        runStatusEl.classList.add('completed');
    } else {
        runStatusEl.textContent = 'Duraklatıldı';
        runStatusEl.classList.add('idle');
    }
}

function updateControls() {
    startBtn.disabled = !isConnected || snapshotState.isRunning;
    pauseBtn.disabled = !isConnected || !snapshotState.isRunning;
    stepBtn.disabled = !isConnected || snapshotState.isRunning;
    resetBtn.disabled = !isConnected;
}

function normalizeResult(result) {
    return {
        year: result.year,
        totalPopulation: result.totalPopulation ?? 0,
        aliveHumans: result.aliveHumans ?? 0,
        aliveAnimals: result.aliveAnimals ?? 0,
        marriages: result.marriages ?? 0,
        births: result.births ?? 0,
        deaths: result.deaths ?? 0,
        accidents: result.accidents ?? 0,
        eventLog: result.eventLog ?? [],
    };
}

function normalizeResults(raw) {
    return (raw ?? []).map(normalizeResult);
}

function updateCharts() {
    const labels = history.map((item) => item.year);
    const totals = history.map((item) => item.totalPopulation);
    const humans = history.map((item) => item.aliveHumans);
    const animals = history.map((item) => item.aliveAnimals);

    populationChart.data.labels = labels;
    populationChart.data.datasets[0].data = totals;
    populationChart.update();

    compositionChart.data.labels = labels;
    compositionChart.data.datasets[0].data = humans;
    compositionChart.data.datasets[1].data = animals;
    compositionChart.update();

    eventsChart.data.labels = history.map((item) => item.year);
    eventsChart.data.datasets = [
        {
            label: 'Evlilik',
            data: history.map((item) => item.marriages),
            borderColor: '#f472b6',
            backgroundColor: 'rgba(244, 114, 182, 0.3)',
        },
        {
            label: 'Doğum',
            data: history.map((item) => item.births),
            borderColor: '#facc15',
            backgroundColor: 'rgba(250, 204, 21, 0.25)',
        },
        {
            label: 'Ölüm',
            data: history.map((item) => item.deaths),
            borderColor: '#94a3b8',
            backgroundColor: 'rgba(148, 163, 184, 0.25)',
        },
        {
            label: 'Kaza',
            data: history.map((item) => item.accidents),
            borderColor: '#22d3ee',
            backgroundColor: 'rgba(34, 211, 238, 0.25)',
        },
    ];
    eventsChart.update();
}

function renderEventLog() {
    eventLogContainer.innerHTML = '';

    history.forEach((result) => {
        if (!result.eventLog || result.eventLog.length === 0) {
            return;
        }

        const article = document.createElement('article');
        const heading = document.createElement('h4');
        heading.textContent = `${result.year}`;
        article.appendChild(heading);

        const list = document.createElement('ul');
        result.eventLog.forEach((entry) => {
            const item = document.createElement('li');
            item.textContent = entry;
            list.appendChild(item);
        });
        article.appendChild(list);

        eventLogContainer.appendChild(article);
    });

    if (!eventLogContainer.hasChildNodes()) {
        const emptyState = document.createElement('p');
        emptyState.textContent = 'Görüntülenecek olay bulunamadı.';
        eventLogContainer.appendChild(emptyState);
    }
}

function updateStats() {
    if (snapshotState.startYear != null) {
        startYearEl.textContent = snapshotState.startYear;
    } else {
        startYearEl.textContent = '-';
    }

    if (snapshotState.currentYear != null) {
        currentYearEl.textContent = snapshotState.currentYear;
    } else {
        currentYearEl.textContent = '-';
    }

    const latest = history.length ? history[history.length - 1] : null;

    totalPopulationEl.textContent = latest ? numberFormatter.format(latest.totalPopulation) : '-';
    aliveHumansEl.textContent = latest ? numberFormatter.format(latest.aliveHumans) : '-';
    aliveAnimalsEl.textContent = latest ? numberFormatter.format(latest.aliveAnimals) : '-';
}

function applySnapshot(snapshot) {
    snapshotState = {
        startYear: snapshot.startYear ?? null,
        currentYear: snapshot.currentYear ?? null,
        isRunning: snapshot.isRunning ?? false,
    };

    history = normalizeResults(snapshot.history);

    updateCharts();
    renderEventLog();
    updateStats();
    updateRunStatus();
    updateControls();
}

function applyYearUpdate(update) {
    snapshotState.currentYear = update.currentYear ?? snapshotState.currentYear;
    snapshotState.isRunning = update.isRunning ?? snapshotState.isRunning;

    if (update.yearResult) {
        const normalized = normalizeResult(update.yearResult);
        const index = history.findIndex((item) => item.year === normalized.year);
        if (index >= 0) {
            history[index] = normalized;
        } else {
            history.push(normalized);
        }
        history.sort((a, b) => a.year - b.year);
    }

    updateCharts();
    renderEventLog();
    updateStats();
    updateRunStatus();
    updateControls();
}

function registerConnectionHandlers() {
    connection.on('ReceiveState', (payload) => {
        applySnapshot(payload);
    });

    connection.on('ReceiveYear', (payload) => {
        applyYearUpdate(payload);
    });

    connection.onreconnecting(() => {
        isConnected = false;
        setConnectionStatus('reconnecting', 'Bağlantı yeniden kuruluyor…');
        updateControls();
    });

    connection.onreconnected(() => {
        isConnected = true;
        setConnectionStatus('online', 'SignalR bağlantısı yeniden kuruldu');
        updateControls();
    });

    connection.onclose(() => {
        isConnected = false;
        setConnectionStatus('offline', 'Bağlantı kapandı, tekrar bağlanılacak.');
        updateControls();
        setTimeout(startConnection, 3000);
    });
}

async function startConnection() {
    try {
        await connection.start();
        isConnected = true;
        setConnectionStatus('online', 'SignalR bağlantısı hazır.');
        updateControls();
        await connection.invoke('ResetSimulation');
    } catch (error) {
        console.error('SignalR bağlantısı kurulamadı', error);
        isConnected = false;
        setConnectionStatus('offline', 'Bağlantı kurulamadı, tekrar denenecek…');
        updateControls();
        setTimeout(startConnection, 3000);
    }
}

async function sendCommand(methodName) {
    if (!isConnected) {
        return;
    }

    try {
        await connection.invoke(methodName);
    } catch (error) {
        console.error(`${methodName} çağrılırken hata oluştu`, error);
        setConnectionStatus('error', 'Komut gönderilemedi. Logları kontrol edin.');
        updateControls();
    }
}

function registerButtonHandlers() {
    startBtn.addEventListener('click', () => sendCommand('StartSimulation'));
    pauseBtn.addEventListener('click', () => sendCommand('PauseSimulation'));
    stepBtn.addEventListener('click', () => sendCommand('StepSimulation'));
    resetBtn.addEventListener('click', () => sendCommand('ResetSimulation'));
}

function bootstrap() {
    initCharts();
    registerButtonHandlers();

    const hubUrl = resolveHubUrl();
    connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    registerConnectionHandlers();
    setConnectionStatus('offline', 'Bağlantı bekleniyor…');
    updateRunStatus();
    updateControls();
    startConnection();
}

bootstrap();
