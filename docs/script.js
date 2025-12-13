// Smooth scroll for internal links
document.querySelectorAll('a[href^="#"]').forEach(link => {
  link.addEventListener('click', e => {
    const id = link.getAttribute('href').substring(1);
    const el = document.getElementById(id);
    if (el) {
      e.preventDefault();
      el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  });
});

// GitHub repository info
const repoOwner = 'yourusername'; // replace with your GitHub username
const repoName = 'CatsuBrowser';  // replace with your repo name

// Fetch latest release from GitHub API
async function updateLatestReleaseButton() {
  const btn = document.getElementById('latest-release');
  if (!btn) return;
  try {
    const res = await fetch(`https://api.github.com/repos/${repoOwner}/${repoName}/releases/latest`);
    if (!res.ok) return;
    const json = await res.json();
    const tag = json.tag_name || json.name;
    if (tag) btn.textContent = `Latest release (${tag})`;
    if (Array.isArray(json.assets) && json.assets.length > 0) {
      btn.href = json.assets[0].browser_download_url;
    }
  } catch (err) {
    console.error('Failed to fetch release info', err);
  }
}
updateLatestReleaseButton();
