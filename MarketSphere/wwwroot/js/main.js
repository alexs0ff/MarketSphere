let currentSearchResult = null;
let timerId = null;
function findItems() {
    let searchTextBox = document.getElementById('searchBox');
    let findProgressBtn = document.getElementById('findProgressBtn');
    let findBtn = document.getElementById('findBtn');

    findBtn.style.display = 'none';
    findProgressBtn.style.display = 'block';

    fetch("/search?query=" + searchTextBox.value, {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-type': 'application/json'
        }
    }).then(response => response.json())
        .then(result => {
            currentSearchResult = result;
            //build();
            build2();
        })
        .catch(error => console.error("Error on search", error));
}

function startSummarize(itemUrl) {
    let summaryBox = document.getElementById('summary');
    summaryBox.scrollIntoView();
    fetch("/start?itemUrl=" + itemUrl, {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-type': 'application/json'
        }
    }).then(response => response.json())
        .then(result => {
            var summaryBox = document.getElementById('summary');
            summaryBox.innerHTML = "Формируем…";
            update();
        })
        .catch(error => console.error("Error on summarize", error));
}


function build() {

    let itemxBox = document.getElementById('items');
    let output = "";
    for (var i = 0; i < currentSearchResult.items.length; i++) {
        output +=
            `
            <div class="col">
                    <div class="card mb-4 rounded-3 shadow-sm border-primary">
                        <div class="card-header py-3 text-bg-primary border-primary">
                            <h4 class="my-0 fw-normal">${currentSearchResult.items[i].name}</h4>
                        </div>
                        <div class="card-body">
                            <h1 class="card-title pricing-card-title">${currentSearchResult.items[i].price} ₽<small class="text-body-secondary fw-light">/шт.</small></h1>
                            <img src="/image?url=${currentSearchResult.items[i].img}" class="d-block w-100">
                            <button type="button" class="w-100 btn btn-lg btn-primary" onclick="startSummarize('${currentSearchResult.items[i].url}')">Отзывы</button>
                        </div>
                    </div>
                </div>
            `;
    }
    itemxBox.innerHTML = output;
}

function build2() {

    let itemxBox = document.getElementById('items');
    let output = "";
    for (var i = 0; i < currentSearchResult.items.length; i++) {
        let isDisabled = 'disabled';
        if (currentSearchResult.items[i].hasComments === true) {
            isDisabled = '';
        }
        output +=
            ` <div class="col">
               <div class="card" style="width: 18rem;">
              <img src="/image?url=${currentSearchResult.items[i].img}" class="card-img-top">
                  <div class="card-body">
                    <h5 class="card-title">${currentSearchResult.items[i].price} ₽<small class="text-body-secondary fw-light">/шт.</small></h5>
                    <p class="card-text">${currentSearchResult.items[i].name}</p>
                    <button type="button" class="w-100 btn btn-primary ${isDisabled}" onclick="startSummarize('${currentSearchResult.items[i].url}')">Отзывы</button>
                  </div>
            </div>
         </div>
            `;
    }
    itemxBox.innerHTML = output;

    let findProgressBtn = document.getElementById('findProgressBtn');
    let findBtn = document.getElementById('findBtn');

    findBtn.style.display = 'block';
    findProgressBtn.style.display = 'none';
}

function update() {
    if (timerId != null) {
        clearInterval(timerId);
        timerId = null;
    }
    timerId = setInterval(() => fetchCurrentSummary(), 1000);
}

function fetchCurrentSummary() {
    fetch("/summary", {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-type': 'application/json'
        }
    }).then(response => response.json())
        .then(result => {
            var summaryBox = document.getElementById('summary');

            if (result.text.length > 0) {
                summaryBox.innerHTML = result.text.replace(/(?:\r\n|\r|\n)/g, '<br>');;
            } else {
                summaryBox.innerHTML = "Формируем…";
            }
        })
        .catch(error => console.error("Error on summarize", error));
}
