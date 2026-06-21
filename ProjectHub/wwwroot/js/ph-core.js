// ProjectHub  -  Copyright (c) 2026 Dr Shuo Ding <shuoding@outlook.com>
// Licensed under the GNU Affero General Public License v3.0 or later (AGPL-3.0-or-later).
// Free to use. Any copy, modification, or distribution must retain this author
// copyright notice and remain under the AGPL. See the LICENSE file for full terms.

/* ===========================================================================
   ph-core.js - shared helpers, view tabs, the task panel (edit + create) with
   MULTIPLE assignees and MULTIPLE dependencies, notes, save/create, confirm
   modal. Deleting a task ARCHIVES it (recoverable) rather than destroying it.
   =========================================================================== */
(function () {
    const PH = window.PH || {};
    window.PH = PH;
    const $ = id => document.getElementById(id);

    function tokenHeader() {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? { 'RequestVerificationToken': el.value } : {};
    }
    async function post(handler, data) {
        const res = await fetch(location.pathname + '?handler=' + handler, {
            method: 'POST',
            headers: Object.assign({ 'Content-Type': 'application/x-www-form-urlencoded' }, tokenHeader()),
            body: new URLSearchParams(data)
        });
        if (!res.ok) throw new Error('Request failed: ' + res.status);
        return res.json();
    }
    async function get(handler, params) {
        const qs = new URLSearchParams(params).toString();
        const res = await fetch(location.pathname + '?handler=' + handler + (qs ? '&' + qs : ''));
        if (!res.ok) throw new Error('Request failed: ' + res.status);
        return res.json();
    }
    PH.api = { post, get };
    function fmt(d) { return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0'); }

    /* confirm modal */
    const cScrim = $('confirmScrim');
    let onConfirm = null;
    PH.confirm = function (msg, cb, okLabel) {
        if ($('confirmMsg')) $('confirmMsg').textContent = msg;
        if ($('confirmOk') && okLabel) $('confirmOk').textContent = okLabel;
        onConfirm = cb; cScrim.classList.remove('hidden');
    };
    if (cScrim) {
        $('confirmCancel').addEventListener('click', () => cScrim.classList.add('hidden'));
        $('confirmOk').addEventListener('click', () => { cScrim.classList.add('hidden'); if (onConfirm) onConfirm(); });
        cScrim.addEventListener('click', e => { if (e.target === cScrim) cScrim.classList.add('hidden'); });
    }

    /* view tabs */
    document.querySelectorAll('.view-tabs .tab').forEach(tab => {
        tab.addEventListener('click', () => {
            document.querySelectorAll('.view-tabs .tab').forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            const v = tab.dataset.view;
            document.querySelectorAll('.view').forEach(sec => sec.classList.toggle('hidden', sec.dataset.view !== v));
            if (PH.renderView) PH.renderView(v);
        });
    });

    /* task panel */
    const panel = $('taskPanel'), scrim = $('panelScrim');
    function setChecks(containerId, ids) {
        const set = new Set((ids || []).map(String));
        document.querySelectorAll('#' + containerId + ' input[type=checkbox]').forEach(cb => { cb.checked = set.has(cb.value); });
    }
    function getChecks(containerId) {
        return Array.from(document.querySelectorAll('#' + containerId + ' input:checked')).map(cb => cb.value).join(',');
    }
    function setFields(t) {
        $('f-taskId').value = t.id || '';
        $('f-title').value = t.title || '';
        $('f-desc').value = t.description || '';
        $('f-status').value = t.status != null ? t.status : 0;
        $('f-priority').value = t.priority != null ? t.priority : 1;
        $('f-start').value = t.start; $('f-due').value = t.due;
        $('f-progress').value = t.progress || 0; $('f-progress-val').textContent = t.progress || 0;
        setChecks('f-assignees', t.assigneeIds);
        setChecks('f-dependsOn', t.dependsOnIds);
        $('f-milestone').checked = !!t.isMilestone;
        // a task cannot depend on itself: hide its own row when editing
        const sid = String(t.id || '');
        document.querySelectorAll('#f-dependsOn .achk').forEach(l => { l.style.display = (sid !== '' && l.dataset.id === sid) ? 'none' : ''; });
    }
    function show() { panel.classList.remove('hidden'); scrim.classList.remove('hidden'); }
    function closePanel() { panel.classList.add('hidden'); scrim.classList.add('hidden'); }
    PH.closePanel = closePanel;

    async function openTask(id) {
        const data = await get('Task', { taskId: id });
        setFields(data.task); renderNotes(data.notes);
        $('panelTitle').textContent = 'Task details';
        $('notesWrap').classList.remove('hidden');
        $('f-delete').classList.remove('hidden');
        show();
    }
    PH.openTask = openTask;

    PH.openCreate = function (group, value) {
        const t = new Date(); const due = new Date(); due.setDate(due.getDate() + 3);
        setFields({ id: '', title: '', description: '', status: 0, priority: 1, progress: 0,
            start: fmt(t), due: fmt(due), assigneeIds: [], dependsOnIds: [], isMilestone: false });
        if (group === 'status') $('f-status').value = value;
        else if (group === 'priority') $('f-priority').value = value;
        else if (group === 'assignee' && value) setChecks('f-assignees', [value]);
        $('panelTitle').textContent = 'New task';
        $('notesWrap').classList.add('hidden');
        $('f-delete').classList.add('hidden');
        show();
    };

    if (panel) {
        $('panelClose').addEventListener('click', closePanel);
        scrim.addEventListener('click', closePanel);
        $('f-progress').addEventListener('input', e => { $('f-progress-val').textContent = e.target.value; });
    }

    /* notes */
    function noteEl(n) {
        const d = document.createElement('div'); d.className = 'note';
        d.innerHTML = '<div class="note-meta"><b></b><span></span></div><div class="note-body"></div>';
        d.querySelector('b').textContent = n.author;
        d.querySelector('.note-meta span').textContent = n.createdAt;
        d.querySelector('.note-body').textContent = n.body;
        return d;
    }
    function renderNotes(notes) {
        const list = $('notesList'); list.innerHTML = '';
        if (!notes || !notes.length) { list.innerHTML = '<p class="muted">No notes yet.</p>'; return; }
        notes.forEach(n => list.appendChild(noteEl(n)));
    }
    if ($('noteForm')) $('noteForm').addEventListener('submit', async e => {
        e.preventDefault();
        const body = $('f-note').value.trim(); if (!body) return;
        const id = $('f-taskId').value; if (!id) return;
        const r = await post('AddNote', { taskId: id, author: 'You', body });
        if (r.ok) {
            const list = $('notesList'); if (list.querySelector('.muted')) list.innerHTML = '';
            list.appendChild(noteEl(r.note)); $('f-note').value = '';
            if (PH.onNoteAdded) PH.onNoteAdded(id);
        }
    });

    /* save (create or edit) */
    function gather() {
        return {
            title: $('f-title').value, description: $('f-desc').value,
            status: $('f-status').value, priority: $('f-priority').value, progress: $('f-progress').value,
            isMilestone: $('f-milestone').checked, start: $('f-start').value, due: $('f-due').value,
            assigneeIds: getChecks('f-assignees'), dependsOnIds: getChecks('f-dependsOn')
        };
    }
    if ($('f-save')) $('f-save').addEventListener('click', async () => {
        const id = $('f-taskId').value; const base = gather();
        if (!base.title.trim()) { $('f-title').focus(); return; }
        let r;
        if (!id) r = await post('CreateTask', Object.assign({ projectId: PH.projectId }, base));
        else r = await post('SaveTask', Object.assign({ taskId: id }, base));
        if (r.ok) { if (PH.onTaskSaved) PH.onTaskSaved(r.task, !id); closePanel(); }
    });

    /* delete = archive (recoverable) */
    function doArchive(id) {
        post('ArchiveTask', { taskId: id }).then(r => { if (r.ok) { if (PH.onTaskArchived) PH.onTaskArchived(id); closePanel(); } });
    }
    PH.doArchive = doArchive;
    if ($('f-delete')) $('f-delete').addEventListener('click', () => {
        const id = $('f-taskId').value; if (!id) return;
        PH.confirm('This task will be moved to the Archive. You can recover it later from the Archive tab.', () => doArchive(id), 'Move to Archive');
    });

    /* delegated clicks */
    document.addEventListener('click', e => {
        const add = e.target.closest('.col-add');
        if (add) { PH.openCreate(add.dataset.group, add.dataset.value); return; }
        const edit = e.target.closest('.js-edit');
        if (edit) { openTask(edit.dataset.id); return; }
        const card = e.target.closest('.task-card');
        if (card) { openTask(card.dataset.id); return; }
        const del = e.target.closest('.js-del');
        if (del) { const id = del.dataset.id; PH.confirm('Move "' + del.dataset.title + '" to the Archive? You can recover it later.', () => doArchive(id), 'Move to Archive'); }
    });

    if (PH.openTaskId) openTask(PH.openTaskId);
})();
