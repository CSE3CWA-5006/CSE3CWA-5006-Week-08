// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

/* ===========================================================================
   ph-board.js - Board + List + Archive from ONE live array. Group by Status /
   Assignee bucket / Priority. Tasks may have multiple assignees (shown under
   each in the assignee bucket) and multiple dependencies (chain badge).
   Deleted tasks live in the Archive tab where they can be recovered or purged.
   =========================================================================== */
(function () {
    const PH = window.PH || {};
    window.PH = PH;
    const { post, get } = PH.api;
    const PRIO = ['low', 'medium', 'high', 'urgent'];

    const boardRoot = document.getElementById('boardRoot');
    if (!boardRoot) return;

    let tasks = JSON.parse(document.getElementById('boardData').textContent || '[]');
    const members = JSON.parse(document.getElementById('membersData').textContent || '[]');
    let group = 'status';
    let sortKey = '', sortAsc = true;

    PH.getTasks = () => tasks;
    function esc(s) { return (s || '').replace(/[&<>"]/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c])); }

    function avatars(list, max) {
        max = max || 3;
        let h = (list || []).slice(0, max).map(a => '<span class="avatar xs" style="background:' + a.color + '" title="' + esc(a.name) + '">' + esc(a.initials) + '</span>').join('');
        if (list && list.length > max) h += '<span class="avatar xs more">+' + (list.length - max) + '</span>';
        return h;
    }

    function columns() {
        if (group === 'priority')
            return [{ v: '3', t: 'Urgent' }, { v: '2', t: 'High' }, { v: '1', t: 'Medium' }, { v: '0', t: 'Low' }];
        if (group === 'assignee') {
            const cols = members.map(m => ({ v: String(m.id), t: m.name }));
            cols.push({ v: '', t: 'Unassigned' });
            return cols;
        }
        return [{ v: '0', t: 'Not started' }, { v: '1', t: 'In progress' }, { v: '2', t: 'Delayed' }, { v: '3', t: 'Completed' }];
    }
    function inColumn(t, v) {
        if (group === 'priority') return String(t.priority) === v;
        if (group === 'assignee') return v === '' ? (t.assignees.length === 0) : t.assignees.some(a => String(a.id) === v);
        return String(t.status) === v;
    }

    function buildCard(t) {
        const card = document.createElement('div');
        card.className = 'task-card'; card.draggable = true; card.dataset.id = t.id;
        const dep = (t.dependsOnIds && t.dependsOnIds.length) ? '<span class="dep-c" title="Depends on ' + t.dependsOnIds.length + ' task(s)">&#128279; ' + t.dependsOnIds.length + '</span>' : '';
        card.innerHTML =
            '<div class="tc-top"><span class="prio p-' + PRIO[t.priority] + '">' + esc(t.priorityName) + '</span>' +
            (t.isMilestone ? '<span class="milestone" title="Milestone">&#9670;</span>' : '') + '</div>' +
            '<div class="tc-title"></div>' +
            '<div class="tc-progress"><div class="bar" style="width:' + t.progress + '%"></div></div>' +
            '<div class="tc-bottom"><span class="due-flag ' + t.dueClass + '">' + esc(t.dueText) + '</span>' +
            '<span class="who">' + dep +
            (t.notesCount > 0 ? '<span class="notes-c">&#128221; ' + t.notesCount + '</span>' : '') +
            '<span class="member-stack">' + avatars(t.assignees) + '</span></span></div>';
        card.querySelector('.tc-title').textContent = t.title;
        wireDrag(card);
        return card;
    }

    function renderBoard() {
        boardRoot.innerHTML = '';
        columns().forEach(col => {
            const items = tasks.filter(t => inColumn(t, col.v));
            const el = document.createElement('div');
            el.className = 'board-col';
            el.innerHTML =
                '<div class="col-head"><span class="col-title">' + esc(col.t) + '</span>' +
                '<span class="col-count">' + items.length + '</span></div><div class="col-body"></div>' +
                '<button class="col-add" data-group="' + group + '" data-value="' + esc(col.v) + '">+ Add task</button>';
            const body = el.querySelector('.col-body');
            el.dataset.value = col.v;
            items.forEach(t => body.appendChild(buildCard(t)));
            wireColumn(el);
            boardRoot.appendChild(el);
        });
    }
    PH.renderBoard = renderBoard;

    function renderList() {
        const body = document.getElementById('listBody');
        if (!body) return;
        let rows = tasks.slice();
        if (sortKey) rows.sort((a, b) => {
            let va, vb;
            if (sortKey === 'priority') { va = a.priority; vb = b.priority; }
            else if (sortKey === 'status') { va = a.status; vb = b.status; }
            else if (sortKey === 'due') { va = a.due; vb = b.due; }
            else { va = (a.title || '').toLowerCase(); vb = (b.title || '').toLowerCase(); }
            return (va > vb ? 1 : va < vb ? -1 : 0) * (sortAsc ? 1 : -1);
        });
        body.innerHTML = '';
        rows.forEach(t => {
            const tr = document.createElement('tr'); tr.dataset.id = t.id;
            tr.innerHTML =
                '<td class="cell-task">' + (t.isMilestone ? '<span class="milestone">&#9670;</span> ' : '') + esc(t.title) + '</td>' +
                '<td><span class="status-pill st-' + t.status + '">' + esc(t.statusText) + '</span></td>' +
                '<td><span class="prio p-' + PRIO[t.priority] + '">' + esc(t.priorityName) + '</span></td>' +
                '<td><span class="member-stack">' + (t.assignees.length ? avatars(t.assignees, 4) : '<span class="muted">Unassigned</span>') + '</span></td>' +
                '<td><span class="due-flag ' + t.dueClass + '">' + esc(t.dueText) + '</span></td>' +
                '<td><div class="row-actions"><button class="btn btn-sm js-edit" data-id="' + t.id + '">Edit</button>' +
                '<button class="btn btn-sm btn-danger js-del" data-id="' + t.id + '" data-title="' + esc(t.title) + '">Delete</button></div></td>';
            body.appendChild(tr);
        });
    }
    PH.renderList = renderList;

    /* ---- Archive view ---- */
    function setArchCount(n) { const el = document.getElementById('archCount'); if (el) el.textContent = n; }
    function bumpArch(delta) { const el = document.getElementById('archCount'); if (el) el.textContent = Math.max(0, (parseInt(el.textContent || '0', 10) + delta)); }

    async function renderArchive() {
        const root = document.getElementById('archiveRoot'); if (!root) return;
        root.innerHTML = '<p class="muted" style="padding:8px">Loading&hellip;</p>';
        const data = await get('Archived', { projectId: PH.projectId });
        const list = data.tasks || [];
        setArchCount(list.length);
        if (!list.length) { root.innerHTML = '<p class="muted" style="padding:8px">The archive is empty.</p>'; return; }
        root.innerHTML = '';
        list.forEach(t => {
            const row = document.createElement('div'); row.className = 'arch-row'; row.dataset.id = t.id;
            row.innerHTML =
                '<span class="status-pill st-' + t.status + '">' + esc(t.statusText) + '</span>' +
                '<span class="attn-title">' + esc(t.title) + '</span>' +
                '<span class="member-stack">' + avatars(t.assignees, 4) + '</span>' +
                '<span class="row-actions"><button class="btn btn-sm js-recover" data-id="' + t.id + '">Recover</button>' +
                '<button class="btn btn-sm btn-danger js-purge" data-id="' + t.id + '" data-title="' + esc(t.title) + '">Delete permanently</button></span>';
            root.appendChild(row);
        });
    }

    PH.renderView = function (v) {
        if (v === 'gantt') { PH.renderGantt && PH.renderGantt(); }
        else if (v === 'list') renderList();
        else if (v === 'archive') renderArchive();
        else renderBoard();
    };
    function activeView() { const v = document.querySelector('.view:not(.hidden)'); return v ? v.dataset.view : 'board'; }
    function renderActive() { PH.renderView(activeView()); }

    /* drag and drop */
    let dragEl = null;
    function wireDrag(card) {
        card.addEventListener('dragstart', () => { dragEl = card; setTimeout(() => card.classList.add('dragging'), 0); });
        card.addEventListener('dragend', () => { card.classList.remove('dragging'); dragEl = null; });
    }
    function afterElement(c, y) {
        const els = Array.from(c.querySelectorAll('.task-card:not(.dragging)'));
        return els.reduce((closest, child) => {
            const box = child.getBoundingClientRect(); const offset = y - box.top - box.height / 2;
            if (offset < 0 && offset > closest.offset) return { offset, element: child };
            return closest;
        }, { offset: -Infinity, element: null }).element;
    }
    function wireColumn(col) {
        const body = col.querySelector('.col-body');
        col.addEventListener('dragover', e => {
            e.preventDefault(); if (!dragEl) return;
            const after = afterElement(body, e.clientY);
            if (after == null) body.appendChild(dragEl); else body.insertBefore(dragEl, after);
        });
        col.addEventListener('drop', async e => {
            e.preventDefault(); if (!dragEl) return;
            const value = col.dataset.value, id = dragEl.dataset.id;
            const ids = Array.from(body.querySelectorAll('.task-card')).map(c => c.dataset.id).join(',');
            dragEl = null;
            try { const r = await post('Move', { taskId: id, group: group, value: value, ids: ids }); if (r.ok && r.task) upsert(r.task); }
            finally { renderActive(); }   // re-render from saved state (card stays + status updated)
        });
    }
    function upsert(t) { const i = tasks.findIndex(x => String(x.id) === String(t.id)); if (i >= 0) tasks[i] = t; else tasks.push(t); }

    PH.onTaskSaved = function (t) { upsert(t); renderActive(); };
    PH.onTaskArchived = function (id) { tasks = tasks.filter(x => String(x.id) !== String(id)); bumpArch(1); renderActive(); };
    PH.onNoteAdded = function (id) { const t = tasks.find(x => String(x.id) === String(id)); if (t) t.notesCount = (t.notesCount || 0) + 1; renderActive(); };

    /* archive actions (delegated) */
    document.addEventListener('click', async e => {
        const rec = e.target.closest('.js-recover');
        if (rec) { const r = await post('RecoverTask', { taskId: rec.dataset.id }); if (r.ok) { if (r.task) upsert(r.task); bumpArch(-1); rec.closest('.arch-row').remove(); } return; }
        const purge = e.target.closest('.js-purge');
        if (purge) {
            PH.confirm('Permanently delete "' + purge.dataset.title + '"? This cannot be undone.', async () => {
                const r = await post('PurgeTask', { taskId: purge.dataset.id });
                if (r.ok) { bumpArch(-1); purge.closest('.arch-row').remove(); }
            }, 'Delete permanently');
            return;
        }
    });
    const emptyBtn = document.getElementById('emptyArchive');
    if (emptyBtn) emptyBtn.addEventListener('click', () => {
        PH.confirm('Permanently delete ALL archived tasks in this project? This cannot be undone.', async () => {
            const r = await post('EmptyArchive', { projectId: PH.projectId });
            if (r.ok) { setArchCount(0); const root = document.getElementById('archiveRoot'); if (root) root.innerHTML = '<p class="muted" style="padding:8px">The archive is empty.</p>'; }
        }, 'Empty archive');
    });

    const sel = document.getElementById('groupBy');
    if (sel) sel.addEventListener('change', () => { group = sel.value; renderBoard(); });
    document.querySelectorAll('.view-list th[data-sort]').forEach(th => {
        th.addEventListener('click', () => {
            const k = th.dataset.sort;
            if (sortKey === k) sortAsc = !sortAsc; else { sortKey = k; sortAsc = true; }
            renderList();
        });
    });

    renderBoard();
})();
