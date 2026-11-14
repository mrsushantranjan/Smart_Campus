// Smart Campus Service Portal - Interactive REST API Integration
(function () {
    "use strict";

    function showToast(message, isError) {
        const container = document.getElementById("portalToastContainer");
        if (!container) return;

        const toastEl = document.createElement("div");
        toastEl.className = `toast align-items-center text-bg-${isError ? "danger" : "success"} border-0`;
        toastEl.setAttribute("role", "alert");
        toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${escapeHtml(message)}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>`;
        container.appendChild(toastEl);

        if (window.bootstrap && window.bootstrap.Toast) {
            const toast = new window.bootstrap.Toast(toastEl, { delay: 3500 });
            toast.show();
            toastEl.addEventListener("hidden.bs.toast", () => toastEl.remove());
        }
    }

    function escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text ?? "";
        return div.innerHTML;
    }

    // 1. Quick Ticket Tracker on Dashboard (GET /api/requests/track/{ticketCode})
    const quickTrackForm = document.getElementById("quickTrackForm");
    if (quickTrackForm) {
        quickTrackForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const input = document.getElementById("quickTrackInput");
            const resultBox = document.getElementById("quickTrackResult");
            const code = (input?.value || "").trim();
            if (!code || !resultBox) return;

            resultBox.innerHTML = `<span class="text-muted">Looking up <strong>${escapeHtml(code)}</strong> via API...</span>`;
            try {
                const res = await fetch(`/api/requests/track/${encodeURIComponent(code)}`);
                if (!res.ok) {
                    resultBox.innerHTML = `<span class="text-danger">Ticket <strong>${escapeHtml(code)}</strong> not found.</span>`;
                    return;
                }
                const data = await res.json();
                resultBox.innerHTML = `
                    <div class="portal-card p-3 d-flex flex-wrap justify-content-between align-items-center gap-2">
                        <div>
                            <span class="ticket-code me-2">${escapeHtml(data.ticketCode)}</span>
                            <span class="fw-medium">${escapeHtml(data.title)}</span>
                            <span class="small text-muted ms-2">${escapeHtml(data.serviceName)} &middot; ${escapeHtml(data.status)}</span>
                        </div>
                        <a href="/Requests/Details/${data.id}" class="btn btn-sm btn-outline-primary">Open Ticket</a>
                    </div>`;
            } catch {
                quickTrackForm.submit();
            }
        });
    }

    // 2. Live Dashboard KPI Refresh (GET /api/dashboard/summary)
    const refreshBtn = document.getElementById("refreshDashboardBtn");
    if (refreshBtn) {
        refreshBtn.addEventListener("click", async () => {
            refreshBtn.disabled = true;
            refreshBtn.textContent = "Refreshing...";
            try {
                const res = await fetch("/api/dashboard/summary");
                if (!res.ok) throw new Error("Failed");
                const data = await res.json();

                const map = {
                    kpiTotalRequests: data.totalRequests,
                    kpiOpenRequests: data.openRequests,
                    kpiInProgressRequests: data.inProgressRequests,
                    kpiResolvedRequests: data.resolvedRequests,
                    kpiUpcomingBookings: data.upcomingBookings,
                    kpiUrgentOpen: data.urgentOpenRequests
                };
                Object.entries(map).forEach(([id, val]) => {
                    const el = document.getElementById(id);
                    if (el) el.textContent = val;
                });

                const breakdownEl = document.getElementById("categoryBreakdownList");
                if (breakdownEl && Array.isArray(data.categoryBreakdown)) {
                    const maxCat = Math.max(1, ...data.categoryBreakdown.map(c => c.requestCount || 0));
                    breakdownEl.innerHTML = data.categoryBreakdown.map(item => {
                        const pct = Math.round((item.requestCount / maxCat) * 100);
                        return `
                            <div>
                                <div class="d-flex justify-content-between small mb-1">
                                    <span class="fw-semibold">${escapeHtml(item.categoryName)}</span>
                                    <span class="text-muted">${item.requestCount}</span>
                                </div>
                                <div class="progress" style="height: 7px;">
                                    <div class="progress-bar bg-primary" role="progressbar" style="width: ${pct}%;"></div>
                                </div>
                            </div>`;
                    }).join("");
                }

                showToast("Dashboard KPIs refreshed from /api/dashboard/summary.", false);
            } catch {
                showToast("Unable to refresh dashboard KPIs.", true);
            } finally {
                refreshBtn.disabled = false;
                refreshBtn.textContent = "Refresh metrics";
            }
        });
    }

    // 3. Live Request Status Update (PATCH /api/requests/{id}/status)
    const apiStatusForm = document.getElementById("apiStatusForm");
    if (apiStatusForm) {
        apiStatusForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const requestId = apiStatusForm.dataset.requestId;
            const status = document.getElementById("statusSelect")?.value;
            const assignedTo = document.getElementById("assignedToInput")?.value;
            const note = document.getElementById("statusNoteInput")?.value;

            try {
                const res = await fetch(`/api/requests/${requestId}/status`, {
                    method: "PATCH",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        status,
                        assignedTo,
                        note,
                        updatedBy: "Campus Operations"
                    })
                });
                if (!res.ok) throw new Error("Status update failed");
                const updated = await res.json();

                const badge = document.getElementById("detailStatusBadge");
                if (badge) badge.textContent = updated.status;
                const assignee = document.getElementById("detailAssignedTo");
                if (assignee) assignee.textContent = updated.assignedTo || "Unassigned";

                renderComments(updated.comments);
                const noteEl = document.getElementById("statusNoteInput");
                if (noteEl) noteEl.value = "";
                showToast(`Ticket ${updated.ticketCode} updated to ${updated.status}.`, false);
            } catch {
                showToast("Failed to update status via API.", true);
            }
        });
    }

    // 4. Live Request Comment Posting (POST /api/requests/{id}/comments)
    const apiCommentForm = document.getElementById("apiCommentForm");
    if (apiCommentForm) {
        apiCommentForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const requestId = apiCommentForm.dataset.requestId;
            const authorName = document.getElementById("commentAuthorName")?.value?.trim();
            const authorRole = document.getElementById("commentAuthorRole")?.value?.trim() || "Requester";
            const message = document.getElementById("commentMessage")?.value?.trim();
            if (!authorName || !message) return;

            try {
                const res = await fetch(`/api/requests/${requestId}/comments`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ authorName, authorRole, message })
                });
                if (!res.ok) throw new Error("Comment failed");
                const detailRes = await fetch(`/api/requests/${requestId}`);
                if (detailRes.ok) {
                    const updated = await detailRes.json();
                    renderComments(updated.comments);
                }
                document.getElementById("commentMessage").value = "";
                showToast("Comment posted via REST API.", false);
            } catch {
                showToast("Unable to post comment via API.", true);
            }
        });
    }

    function renderComments(comments) {
        const timeline = document.getElementById("commentsTimeline");
        const countEl = document.getElementById("commentCount");
        if (!timeline || !Array.isArray(comments)) return;
        if (countEl) countEl.textContent = comments.length;

        timeline.innerHTML = comments.map(c => `
            <div class="timeline-item ${c.isSystemGenerated ? "timeline-system" : ""}">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <span class="fw-semibold small">${escapeHtml(c.authorName)}</span>
                        <span class="badge bg-light text-dark border ms-1">${escapeHtml(c.authorRole)}</span>
                    </div>
                    <span class="small text-muted">${new Date(c.createdAt).toLocaleString()}</span>
                </div>
                <div class="small mt-1">${escapeHtml(c.message)}</div>
            </div>`).join("");
    }

    // 5. Live Facility Slot Availability Check (GET /api/facilities/{id}/availability?date=...)
    const facSelect = document.getElementById("bookingFacilitySelect");
    const dateInput = document.getElementById("bookingDateInput");
    const availPanel = document.getElementById("availabilityPanel");

    async function loadFacilityAvailability() {
        if (!facSelect || !dateInput || !availPanel) return;
        const facilityId = facSelect.value;
        const dateVal = dateInput.value;
        if (!facilityId || !dateVal) return;

        availPanel.innerHTML = `<span class="text-muted">Checking booked slots...</span>`;
        try {
            const res = await fetch(`/api/facilities/${encodeURIComponent(facilityId)}/availability?date=${encodeURIComponent(dateVal)}`);
            if (!res.ok) throw new Error("Failed");
            const data = await res.json();

            let slotsHtml = "";
            if (!data.bookedSlots || data.bookedSlots.length === 0) {
                slotsHtml = `<div class="alert alert-success py-2 px-3 mb-0">No conflicting bookings on <strong>${escapeHtml(data.date)}</strong>. All operating hours are open!</div>`;
            } else {
                slotsHtml = `
                    <div class="fw-semibold mb-2 text-danger">Reserved Slots on ${escapeHtml(data.date)}:</div>
                    <ul class="list-group list-group-flush border rounded-2">
                        ${data.bookedSlots.map(s => `
                            <li class="list-group-item d-flex justify-content-between align-items-center py-2">
                                <div>
                                    <div class="fw-semibold">${escapeHtml(s.startTime)} - ${escapeHtml(s.endTime)}</div>
                                    <div class="text-muted">${escapeHtml(s.eventTitle)}</div>
                                </div>
                                <span class="badge bg-warning text-dark">${escapeHtml(s.status)}</span>
                            </li>`).join("")}
                    </ul>`;
            }

            availPanel.innerHTML = `
                <div class="mb-2">
                    <div class="fw-bold">${escapeHtml(data.facilityName)}</div>
                    <div class="text-muted">Operating Hours: ${escapeHtml(data.openTime)} - ${escapeHtml(data.closeTime)}</div>
                </div>
                ${slotsHtml}`;
        } catch {
            availPanel.innerHTML = `<span class="text-danger">Could not fetch live availability.</span>`;
        }
    }

    if (facSelect && dateInput && availPanel) {
        facSelect.addEventListener("change", loadFacilityAvailability);
        dateInput.addEventListener("change", loadFacilityAvailability);
        loadFacilityAvailability();
    }

    // 6. REST API Explorer Page Tester
    document.querySelectorAll(".api-try-btn").forEach(btn => {
        btn.addEventListener("click", async () => {
            const endpoint = btn.dataset.endpoint;
            const statusBadge = document.getElementById("apiPreviewStatus");
            const urlEl = document.getElementById("apiPreviewUrl");
            const outEl = document.getElementById("apiPreviewOutput");
            if (!endpoint || !outEl) return;

            if (urlEl) urlEl.textContent = `GET ${endpoint}`;
            if (statusBadge) {
                statusBadge.className = "badge bg-warning text-dark";
                statusBadge.textContent = "Loading...";
            }
            try {
                const res = await fetch(endpoint);
                const json = await res.json();
                if (statusBadge) {
                    statusBadge.className = `badge ${res.ok ? "bg-success" : "bg-danger"}`;
                    statusBadge.textContent = `${res.status} ${res.statusText}`;
                }
                outEl.textContent = JSON.stringify(json, null, 2);
            } catch (err) {
                if (statusBadge) {
                    statusBadge.className = "badge bg-danger";
                    statusBadge.textContent = "Error";
                }
                outEl.textContent = String(err);
            }
        });
    });
})();
