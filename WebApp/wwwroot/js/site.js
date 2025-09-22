// site.js

document.addEventListener('DOMContentLoaded', () => {
    // ===== Tooltip Bootstrap =====
    if (window.bootstrap) {
        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => new bootstrap.Tooltip(el));
    }

    // ===== THEME CONTROLLER (single source of truth) =====
    const THEME_KEY = 'theme';

    // elements (có thể không tồn tại — guard an toàn)
    const headerToggleBtn = document.getElementById('themeToggle');
    const iconSun = document.getElementById('iconSun');
    const iconMoon = document.getElementById('iconMoon');
    const modalThemeBtns = document.querySelectorAll('.btn-toggle[data-theme]');

    // init theme (ưu tiên localStorage → system)
    const stored = localStorage.getItem(THEME_KEY);
    const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    const initialTheme = stored || (prefersDark ? 'dark' : 'light');

    setTheme(initialTheme);
    reflectActiveButtons(initialTheme);
    syncHeaderIcons(initialTheme);

    // header toggle (nếu có)
    if (headerToggleBtn) {
        headerToggleBtn.addEventListener('click', () => {
            const current = getTheme();
            const next = current === 'light' ? 'dark' : 'light';
            setTheme(next);
            reflectActiveButtons(next);
            syncHeaderIcons(next);
        });
    }

    // modal buttons
    modalThemeBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            modalThemeBtns.forEach(x => x.classList.remove('active'));
            btn.classList.add('active');
            const next = btn.dataset.theme;
            setTheme(next);
            syncHeaderIcons(next);
        });
    });

    // theo dõi system theme *chỉ khi người dùng chưa chọn thủ công*
    if (!stored && window.matchMedia) {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
            const next = e.matches ? 'dark' : 'light';
            setTheme(next);
            reflectActiveButtons(next);
            syncHeaderIcons(next);
        });
    }

    // helpers
    function setTheme(val) {
        document.documentElement.setAttribute('data-theme', val);
        localStorage.setItem(THEME_KEY, val);
    }

    function getTheme() {
        return document.documentElement.getAttribute('data-theme') || 'light';
    }

    function reflectActiveButtons(val) {
        modalThemeBtns.forEach(b => b.classList.toggle('active', b.dataset.theme === val));
    }

    function syncHeaderIcons(val) {
        if (!iconSun || !iconMoon) return; // không có icon thì bỏ qua
        if (val === 'dark') {
            iconMoon.classList.add('d-none');
            iconSun.classList.remove('d-none');
        } else {
            iconSun.classList.add('d-none');
            iconMoon.classList.remove('d-none');
        }
    }

    // ===== LANGUAGE PERSIST (simple) =====
    const langSel = document.getElementById('langSelect');
    if (langSel) {
        const KEY_LANG = 'lang';
        const savedLang = localStorage.getItem(KEY_LANG) || langSel.value;
        langSel.value = savedLang;
        langSel.addEventListener('change', () => localStorage.setItem(KEY_LANG, langSel.value));
    }
});
