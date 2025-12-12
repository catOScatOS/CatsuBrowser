// Simple enhancement: smooth scrolling, version check placeholder

// Smooth scroll for internal links
document.querySelectorAll('a[href^="#"]').forEach(a => {
  a.addEventListener('click', e => {
    const id = a.getAttribute('href').substring(1);
    const el = document.getElementById(id);
    if (el) {
      e.preventDefault();
      el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  });
});

// Basic placeholder to fetch latest release version from GitHub Releases
// Replace `yourusername` with your GitHub username.
const repoOwner = 'yourusername';
const repoName = 'CatsuBrowser';
async function fetchLatestVersion() {
  try {
    const res = await fetch(`https://api.github.com/repos/${repoOwner}/${repoName}/releases/latest`);
    if (!res.ok) return;
    const json = await res.json();
    const versionTag = json.tag_name || json.name;
    const downloadBtn = document.querySelector('#download .btn.primary');
    if (versionTag && downloadBtn) {
      downloadBtn.textContent = `Dernière release (${versionTag})`;
      // Optional: update href to first asset if available
      if (Array.isArray(json.assets) && json.assets.length > 0) {
        downloadBtn.href = json.assets[0].browser_download_url;
      }
    }
  } catch (_) { /* silent */ }
}
fetchLatestVersion();
