const datasetSelect = document.getElementById('dataset-select');
const populationCtx = document.getElementById('populationChart');
const compositionCtx = document.getElementById('compositionChart');
const eventsCtx = document.getElementById('eventsChart');
const eventLogContainer = document.getElementById('event-log');

let populationChart;
let compositionChart;
let eventsChart;

async function loadData(path) {
    try {
        const response = await fetch(path);
        if (!response.ok) {
            throw new Error(`${path} dosyası yüklenemedi (${response.status})`);
        }
        const data = await response.json();
        if (!data || !Array.isArray(data.yearlyResults)) {
            throw new Error('Beklenen veri formatı bulunamadı.');
        }
        return data;
    } catch (error) {
        console.error(error);
        alert(`Veri yüklenirken hata oluştu: ${error.message}`);
        throw error;
    }
}

function buildPopulationChart(ctx, labels, totals) {
    if (populationChart) populationChart.destroy();
    populationChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels,
            datasets: [
                {
                    label: 'Toplam Nüfus',
                    data: totals,
                    fill: true,
                    tension: 0.35,
                    borderColor: '#38bdf8',
                    backgroundColor: 'rgba(56, 189, 248, 0.2)',
                    pointRadius: 4,
                },
            ],
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true,
                },
            },
        },
    });
}

function buildCompositionChart(ctx, labels, humans, animals) {
    if (compositionChart) compositionChart.destroy();
    compositionChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [
                {
                    label: 'İnsan',
                    data: humans,
                    backgroundColor: 'rgba(96, 165, 250, 0.7)',
                    stack: 'stack',
                },
                {
                    label: 'Hayvan',
                    data: animals,
                    backgroundColor: 'rgba(74, 222, 128, 0.7)',
                    stack: 'stack',
                },
            ],
        },
        options: {
            responsive: true,
            scales: {
                x: { stacked: true },
                y: { stacked: true, beginAtZero: true },
            },
        },
    });
}

function buildEventsChart(ctx, labels, marriages, births, deaths, accidents) {
    if (eventsChart) eventsChart.destroy();
    eventsChart = new Chart(ctx, {
        type: 'radar',
        data: {
            labels,
            datasets: [
                {
                    label: 'Evlilik',
                    data: marriages,
                    borderColor: '#f472b6',
                    backgroundColor: 'rgba(244, 114, 182, 0.3)',
                },
                {
                    label: 'Doğum',
                    data: births,
                    borderColor: '#facc15',
                    backgroundColor: 'rgba(250, 204, 21, 0.25)',
                },
                {
                    label: 'Ölüm',
                    data: deaths,
                    borderColor: '#94a3b8',
                    backgroundColor: 'rgba(148, 163, 184, 0.25)',
                },
                {
                    label: 'Kaza',
                    data: accidents,
                    borderColor: '#22d3ee',
                    backgroundColor: 'rgba(34, 211, 238, 0.25)',
                },
            ],
        },
        options: {
            responsive: true,
            elements: {
                line: {
                    borderWidth: 2,
                },
            },
        },
    });
}

function buildEventLog(results) {
    eventLogContainer.innerHTML = '';
    results.forEach((result) => {
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

function normalizeResults(raw) {
    return raw.map((result) => ({
        year: result.year,
        totalPopulation: result.totalPopulation ?? 0,
        aliveHumans: result.aliveHumans ?? 0,
        aliveAnimals: result.aliveAnimals ?? 0,
        marriages: result.marriages ?? 0,
        births: result.births ?? 0,
        deaths: result.deaths ?? 0,
        accidents: result.accidents ?? 0,
        eventLog: result.eventLog ?? [],
    }));
}

async function refreshDashboard(path) {
    const exportData = await loadData(path);
    const normalized = normalizeResults(exportData.yearlyResults);

    const labels = normalized.map((item) => item.year);
    const totals = normalized.map((item) => item.totalPopulation);
    const humans = normalized.map((item) => item.aliveHumans);
    const animals = normalized.map((item) => item.aliveAnimals);
    const marriages = normalized.map((item) => item.marriages);
    const births = normalized.map((item) => item.births);
    const deaths = normalized.map((item) => item.deaths);
    const accidents = normalized.map((item) => item.accidents);

    buildPopulationChart(populationCtx, labels, totals);
    buildCompositionChart(compositionCtx, labels, humans, animals);
    buildEventsChart(eventsCtx, labels, marriages, births, deaths, accidents);
    buildEventLog(normalized);
}

datasetSelect.addEventListener('change', (event) => {
    refreshDashboard(event.target.value);
});

// İlk yüklemede demo datası
refreshDashboard(datasetSelect.value);
