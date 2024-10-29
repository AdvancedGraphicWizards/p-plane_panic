const repoOwner = 'AdvancedGraphicWizards';
const repoName = 'p-plane_panic';
const apiUrl = `https://api.github.com/repos/${repoOwner}/${repoName}`;

fetch(apiUrl)
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        document.getElementById('repo-name').textContent = data.name;
        document.getElementById('repo-description').textContent = data.description;
        document.getElementById('repo-stars-num').textContent = data.stargazers_count;
        document.getElementById('repo-forks-num').textContent = data.forks_count;
        document.getElementById('repo-name').href = data.html_url;
        document.getElementById('repo-icon').href = data.html_url;
    })
    .catch(error => {
        console.error('Error fetching repository data:', error);
    });