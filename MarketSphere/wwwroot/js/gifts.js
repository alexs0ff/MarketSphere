setInterval(() => fetchCurrentFlow(), 1000);


let chatList = [];

function addInitChatItem() {
    chatList.length = 0;
    chatList.push({ 'type': 'bot', 'text': 'Привет! Помогу тебе определиться с подарком, напиши кого ты хочешь порадовать, а я буду подбирать варианты.' })
}

(function () {
    addInitChatItem();
})();

//chatList.push({ 'type': 'user', 'text': 'Давай попробуем' })

let advices = [];


function addAdvice(advice) {
    let exists = false;
    for (var i = 0; i < advices.length; i++) {
        if (advices[i].id == advice.id) {
            exists = true;
        }
    }

    if (exists == false) {
        advices.push(advice);
    }

    return !exists;
}
function fetchCurrentFlow() {
    fetch("/giftsAdvice", {
        method: "GET",
        headers: {
            'Accept': 'application/json',
            'Content-type': 'application/json'
        }
    }).then(response => response.json())
        .then(result => {
            build(result);
        })
        .catch(error => {
            console.error("Error on gift advice", error);
        }
        );
}



function build(flow) {
    let progressBar = document.getElementById('currentProgress');
    let progressBarValue = document.getElementById('currentProgressValue');

    progressBar.setAttribute('aria-valuenow', flow.currentProgress);
    progressBarValue.setAttribute('style', 'width:' + Number(flow.currentProgress) + '%');

    if (flow.classifiedGifts) {
        let table = document.getElementById('classified');
        let output = "";
        for (var i = 0; i < flow.classifiedGifts.length; i++) {
            output +=
                `  <tr>
                      <td>${flow.classifiedGifts[i].gift.name}</td>
                      <td>${flow.classifiedGifts[i].gift.description}</td>
                      <td>${flow.classifiedGifts[i].age}</td>
                      <td>${flow.classifiedGifts[i].hobbies.join(', ')}</td>                  
        </tr>
                `;
        }
        table.innerHTML = output;
    }

    if (flow.advice) {

        let needProcessNewAdvice = addAdvice(flow.advice);
        let textAdvice = "";

        let table = document.getElementById('advice');
        let output = "";

        let adviceProgressBtn = document.getElementById('adviceProgressBtn');
        let adviceBtn = document.getElementById('adviceButton');

        adviceBtn.style.display = 'block';
        adviceProgressBtn.style.display = 'none';

        textAdvice += 'Мы рассмотрели такие варианты для возраста  ' + flow.advice.suggestedAge + ' и увлечений ' + flow.advice.suggestedHobbies.join(', ');
        
        if (flow.advice.gifts && flow.advice.gifts.length) {

            textAdvice += ' и подобрали такие подарки:';
            for (var i = 0; i < flow.advice.gifts.length; i++) {
                output +=
                    `  <tr>
                      <td>${flow.advice.suggestedAge}</td>
                      <td>${flow.advice.suggestedHobbies.join(', ')}</td>
                      <td>${flow.advice.gifts[i].name}</td>
                         </tr>
                `; 
                textAdvice += `<br/> <br/> <a href='https://www.google.com/search?q=${flow.advice.gifts[i].name}'> ${flow.advice.gifts[i].name}</a> стоимостью <mark>${flow.advice.gifts[i].price} ₽ </mark>`;
            }
        } else {
            output +=
                `  <tr>
                      <td>${flow.advice.suggestedAge}</td>
                      <td>${flow.advice.suggestedHobbies.join(', ')}</td>
                      <td>----</td>
                  </tr>
                `;

            textAdvice += '<br/>К сожалению не смогли найти подходящий подарок, попробуйте рассказать подробнее о человеке';
        }
        table.innerHTML = output;

        if (needProcessNewAdvice) {
            chatList.push({ 'type': 'bot', 'text': textAdvice })
        }
    }
    let chat = document.getElementById('chat');
    let output = "";

    if (chatList.length) {

        for (var i = 0; i < chatList.length; i++) {
            let type = chatList[i].type;

            if (type == 'user') {
                output += `
                 <div class="d-flex flex-row justify-content-end mb-4">
                                                        <div class="p-3 me-3 border bg-body-tertiary" style="border-radius: 15px;">
                                                            <p class="small mb-0 text-start">${chatList[i].text}</p>
                                                        </div>
                                                        <img src="/img/chatuser.png"
                                                             alt="avatar 1" style="width: 45px; height: 100%;">
                                                    </div>
                `;
            }
            else if (type == 'bot') {
                output += `
                  <div class="d-flex flex-row justify-content-start mb-4">
                                                        <img src="/img/bot.png"
                                                             alt="avatar 1" style="width: 45px; height: 100%;">
                                                        <div class="p-3 ms-3" style="border-radius: 15px; background-color: rgba(57, 192, 237,.2);">
                                                            <p class="small mb-0 text-start">
                                                                ${chatList[i].text}
                                                            </p>
                                                        </div>
                                                    </div>
                `;
            }
        }
    }

    chat.innerHTML = output;
}

function startParse(url) {
    addInitChatItem();

    let request = {
        "url": url
    };

    fetch("/giftsParse", {
        method: "POST",
        headers: {
            'Accept': 'application/json',
            'Content-type': 'application/json'
        },
        body: JSON.stringify(request)
    }).
        then(result => {
            console.log(result);
        })
        .catch(error => {
            console.error("Error on start parse", error);
        });
}


function startAdvice() {
    let box = document.getElementById('giftTextArea');
    
    if (box.value.length === 0) {
        return;
    }

    let text = '';

    for (var i = 0; i < chatList.length; i++) {
        if (chatList[i].type == 'user') {
            text += ' ' + chatList[i].text;
        }
    }

    chatList.push({ 'type': 'user', 'text': box.value })

    text += ' ' + box.value;

    let request = {
        "text": text,
        "line": box.value
    };
    box.value = '';

    let table = document.getElementById('advice');
    table.innerHTML = "";

    let adviceProgressBtn = document.getElementById('adviceProgressBtn');
    let adviceBtn = document.getElementById('adviceButton');

    adviceBtn.style.display = 'none';
    adviceProgressBtn.style.display = 'block';

    fetch("/advice", {
        method: "POST",
        headers: {
            'Accept': 'application/json',
            'Content-type': 'application/json'
        },
        body: JSON.stringify(request)
    }).then(result => {
            console.log(result);
        })
        .catch(error => {
            console.error("Error on start advice", error);
        });
}