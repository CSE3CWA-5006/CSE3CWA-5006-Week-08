// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

/* ===========================================================================
   ph-gantt.js - Gantt with Day / Week / Month zoom. Reads the SAME live task
   array as the board (PH.getTasks), so tasks added anywhere show up here too.
   Drag a bar to shift its dates.
   =========================================================================== */
(function () {
    const PH = window.PH || {};
    window.PH = PH;
    const { post } = PH.api;

    const DAY = 86400000;
    const PX = { day: 26, week: 12, month: 4 };
    let zoom = 'day', min = null, days = 0;

    function esc(s) { return (s || '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c])); }
    function parse(s) { const p = s.split('-').map(Number); return new Date(p[0], p[1] - 1, p[2]); }
    function fmt(d) { return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0'); }
    const MON = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

    function data() {
        const src = (PH.getTasks ? PH.getTasks() : []);
        return src.map(t => ({ id: t.id, title: t.title, start: t.start, end: t.due, progress: t.progress,
            status: t.status, isMilestone: t.isMilestone,
            initials: (t.assignees && t.assignees.length) ? t.assignees[0].initials : '' }));
    }

    function header(px, d0) {
        const w = days * px;
        let h = '<div class="g-header" style="width:' + w + 'px">';
        if (zoom === 'day') {
            for (let i = 0; i < days; i++) {
                const d = new Date(d0.getTime() + i * DAY);
                const wk = (d.getDay() === 0 || d.getDay() === 6) ? ' weekend' : '';
                h += '<div class="g-tick' + wk + '" style="left:' + (i * px) + 'px;width:' + px + 'px">'
                  + ((d.getDate() === 1 || i === 0) ? '<span class="g-tlabel">' + MON[d.getMonth()] + '</span>' : '')
                  + '<span class="g-tnum">' + d.getDate() + '</span></div>';
            }
        } else if (zoom === 'week') {
            for (let i = 0; i < days; i += 7) {
                const d = new Date(d0.getTime() + i * DAY);
                h += '<div class="g-tick" style="left:' + (i * px) + 'px;width:' + (7 * px) + 'px">'
                  + '<span class="g-tnum">' + MON[d.getMonth()] + ' ' + d.getDate() + '</span></div>';
            }
        } else {
            let i = 0;
            while (i < days) {
                const d = new Date(d0.getTime() + i * DAY);
                const dim = new Date(d.getFullYear(), d.getMonth() + 1, 0).getDate();
                const seg = Math.min(dim - d.getDate() + 1, days - i);
                h += '<div class="g-tick" style="left:' + (i * px) + 'px;width:' + (seg * px) + 'px">'
                  + '<span class="g-tlabel">' + MON[d.getMonth()] + ' ' + d.getFullYear() + '</span></div>';
                i += seg;
            }
        }
        return h + '</div>';
    }

    function render() {
        const root = document.getElementById('ganttRoot');
        if (!root) return;
        const items = data();
        if (!items.length) { root.innerHTML = '<p class="muted" style="padding:16px">No tasks to chart.</p>'; return; }

        min = parse(items[0].start); let max = parse(items[0].end);
        items.forEach(t => { const s = parse(t.start), e = parse(t.end); if (s < min) min = s; if (e > max) max = e; });
        min = new Date(min.getTime() - 2 * DAY); max = new Date(max.getTime() + 2 * DAY);
        days = Math.round((max - min) / DAY) + 1;
        const px = PX[zoom], w = days * px;
        const stepPx = (zoom === 'day') ? px : 7 * px;
        const gridStyle = 'width:' + w + 'px;background-image:linear-gradient(90deg,rgba(98,100,167,.12) 0 1px,transparent 1px);background-size:' + stepPx + 'px 100%';

        let rows = '';
        items.forEach(t => {
            const s = parse(t.start), e = parse(t.end);
            const left = Math.round((s - min) / DAY) * px;
            const width = Math.max(6, (Math.round((e - s) / DAY) + 1) * px);
            if (t.isMilestone) {
                rows += '<div class="g-row"><div class="g-label">&#9670; ' + esc(t.title) + '</div>'
                  + '<div class="g-track" style="' + gridStyle + '"><div class="g-milestone" title="' + esc(t.title) + '" style="left:' + left + 'px"></div></div></div>';
            } else {
                rows += '<div class="g-row"><div class="g-label">' + esc(t.title) + '</div>'
                  + '<div class="g-track" style="' + gridStyle + '"><div class="g-bar s' + t.status + '" data-id="' + t.id + '" style="left:' + left + 'px;width:' + width + 'px">'
                  + '<div class="g-fill" style="width:' + t.progress + '%"></div><span class="g-bartext">' + esc(t.initials) + '</span></div></div></div>';
            }
        });
        root.innerHTML = '<div class="g-scroll"><div class="g-headrow"><div class="g-corner"></div>' + header(px, min) + '</div>'
            + '<div class="g-body">' + rows + '</div></div>';
        enableDrag(px);
    }
    PH.renderGantt = render;

    function enableDrag(px) {
        let drag = null;
        document.querySelectorAll('#ganttRoot .g-bar').forEach(bar => {
            bar.addEventListener('mousedown', e => { drag = { bar, startX: e.clientX, left: parseFloat(bar.style.left) }; bar.classList.add('dragging'); e.preventDefault(); });
        });
        document.addEventListener('mousemove', e => {
            if (!drag) return;
            const snapped = Math.round((drag.left + (e.clientX - drag.startX)) / px) * px;
            drag.bar.style.left = Math.max(0, snapped) + 'px';
        });
        document.addEventListener('mouseup', async () => {
            if (!drag) return;
            const bar = drag.bar; bar.classList.remove('dragging');
            const offset = Math.round(parseFloat(bar.style.left) / px);
            const lenDays = Math.round(bar.offsetWidth / px) - 1;
            const start = new Date(min.getTime() + offset * DAY);
            const end = new Date(start.getTime() + Math.max(0, lenDays) * DAY);
            drag = null;
            const r = await post('UpdateDates', { taskId: bar.dataset.id, start: fmt(start), due: fmt(end) });
            // reflect new dates in the shared array so other views agree
            if (r.ok && PH.getTasks) {
                const t = PH.getTasks().find(x => String(x.id) === String(bar.dataset.id));
                if (t) { t.start = fmt(start); t.due = fmt(end); }
            }
        });
    }

    document.querySelectorAll('.zoom button[data-zoom]').forEach(b => {
        b.addEventListener('click', () => {
            document.querySelectorAll('.zoom button').forEach(x => x.classList.remove('active'));
            b.classList.add('active'); zoom = b.dataset.zoom; render();
        });
    });
})();
