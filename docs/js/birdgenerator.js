function insertBird(memberId, birdColor) {
    const memberElement = document.querySelector(`#${memberId} h5`);
    const birdHTML = `<div class="bird-character" style="--bird-color: ${birdColor};"></div>`;
    memberElement.insertAdjacentHTML('beforeend', birdHTML);
}