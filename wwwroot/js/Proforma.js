/* ============================================================
   Proforma.js  -  Create + Edit Proforma Invoice (single view)
   Usage (in view):  GetProformaForm.init(config);
   ============================================================ */

var GetProformaForm = function () {

    let pfCfg = {};
    let isEdit = false;
    let invoiceId = 0;
    let pfUrls = {};

    let rowCounter = 0;
    let searchTimer = null;
    let skipGstAuto = false;   // stops auto-GST from overwriting saved / imported rates
    let selectedCompany = '';  // company chosen from the search list
    let companyNames = [];     // company list for keyup search

    // ============================================================
    // HELPERS
    // ============================================================

    // first non-null property among several possible casings
    function pick(obj) {
        if (!obj) return undefined;
        for (let i = 1; i < arguments.length; i++) {
            const v = obj[arguments[i]];
            if (v !== undefined && v !== null) return v;
        }
        return undefined;
    }

    function getValue(id) { return $('#' + id).val() || ''; }

    function getNumber(id) {
        const v = parseFloat($('#' + id).val());
        return isNaN(v) ? 0 : v;
    }

    function getRowNumber(row, selector) {
        const v = parseFloat(row.querySelector(selector)?.value);
        return isNaN(v) ? 0 : v;
    }

    function escapeHtml(value) {
        return $('<div>').text(value || '').html();
    }

    function escapeRegex(value) {
        return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    // ============================================================
    // DOCUMENT READY
    // ============================================================

    function initializePage() {

        if (isEdit) {
            // locked in edit mode
            $('#companyName').prop('disabled', true);
            $('#AgainstBy').prop('disabled', true);
        }

        // Company search (keyup) – create only, company is locked in edit
        $('#companyName')
            .off('.pfCompany')
            .on('input.pfCompany', function () { searchCompany(this); })
            .on('keydown.pfCompany', function (e) { handleServiceKeydown(e, this); });

        // Against By
        $('#AgainstBy').on('change', function () {
            loadAgainstByValues();
        });

        // Against By Value
        $('#AgainstByValue').on('change', function () {
            loadAgainstByDetails();
        });

        // Numeric-only inputs
        $(document).on('input', '.numeric', function () {
            this.value = this.value.replace(/[^0-9.]/g, '');
        });

        // Cheque date auto-format yyyy-MM-dd
        $(document).on('keyup', '#txtChequedate', function () {
            formatChequeDate(this);
        });

        // Payment recalculation
        $(document).on('input', '#txtTdsAmount, #txtGSTAmountReceived', function () {
            calculateAll();
        });

        // Bank ADD / DELETE
        $(document).on('click', '.add-row', function () { addBankDetail(); });
        $(document).on('click', '.delete-row', function () { deleteBankDetails(); });

        // Close service dropdown when clicking outside
        $(document).on('click', function (e) {
            if (!$(e.target).closest('.service-wrapper').length) {
                $('.service-dropdown').hide();
            }
        });

        bindPageEvents();

        if (isEdit) initEdit();
        else initCreate();

        calculateAll();
    }

    // ============================================================
    // PAGE EVENTS (row inputs + buttons)
    // ============================================================

    function bindPageEvents() {

        $('#invoiceItems')
            .off('.pfRow')
            .on('input.pfRow', '.service', function () { searchService(this); })
            .on('keydown.pfRow', '.service', function (e) { handleServiceKeydown(e, this); })
            .on('input.pfRow', '.rate', function () { calculateRow(this); })
            .on('input.pfRow', '.cgst-rate', function () { taxChanged(this, 'CGST'); })
            .on('input.pfRow', '.sgst-rate', function () { taxChanged(this, 'SGST'); })
            .on('input.pfRow', '.igst-rate', function () { taxChanged(this, 'IGST'); })
            .on('click.pfRow', '.remove-row', function () { removeRow(this); });

        // Bank table: open tax invoice PDF
        $('#tblBankDetail')
            .off('.pfBank')
            .on('click.pfBank', '.btn-pdf-view', function () {
                openTaxInvoicePdf($(this).data('id'));
            });

        $('#btnAddRow').off('click.pf').on('click.pf', function () { addInvoiceRow(); });
        $('#saveBtn').off('click.pf').on('click.pf', function () { saveInvoice(); });
        $('#btnClear').off('click.pf').on('click.pf', function () { clearInvoice(); });
    }


    // ============================================================
    // INIT – CREATE
    // ============================================================

    function initCreate() {
        $('#AgainstByValue')
            .prop('disabled', true)
            .html('<option value="">-- Select First --</option>');

        setTodayDate();
        addInvoiceRow();
    }

    // ============================================================
    // INIT – EDIT
    // ============================================================

    function initEdit() {

        const m = pfCfg.main || {};

        selectedCompany = ($('#companyName').val() || '').toString().trim();

        // ---- Header ----
        const d = pick(m, 'invoicedate', 'Invoicedate', 'invoiceDate');
        if (d) $('#invoiceDate').val(String(d).substring(0, 10));

        $('#reverseCharge').val(pick(m, 'reversecharge', 'Reversecharge') || 'N');

        // ---- Billing (fall back to Billing* columns if main columns are empty) ----
        const gstin = (pick(m, 'gstIn', 'GstIn', 'BillingGST', 'billingGST') || '').toString().trim();

        $('#gstin').val(gstin);
        $('#address').val(pick(m, 'Address', 'address', 'BillingAddress') || '');
        $('#Location').val(pick(m, 'Location', 'location', 'BillingLocation') || '');
        $('#billingPincode').val(pick(m, 'PinCode', 'pinCode', 'BillingPincode') || '');
        $('#billingState').val(pick(m, 'state', 'State', 'BillingState') || '');
        $('#billingStateCode').val(
            pick(m, 'statecode', 'Statecode', 'BillingStatecode') ||
            (gstin.length >= 2 ? gstin.substring(0, 2) : '')
        );
        $('#companyCode').val(pick(m, 'companyCode', 'CompanyCode') || '');

        // ---- Items (rates are restored as saved, NOT re-derived from GSTIN) ----
        loadExistingDetails();

        // ---- Bank rows ----
        loadExistingBank();

        // ---- Payment inputs ----
        $('#txtGSTAmountReceived').val(pick(m, 'GSTAmountReceived', 'gstAmountReceived') || 0);
        $('#txtTdsAmount').val(pick(m, 'TDSPercentage', 'tdsPercentage') || 0);

        // ---- Against By ----
        const againstBy = pick(m, 'AgainstBy', 'againstBy') || 'Direct';
        $('#AgainstBy').val(againstBy);

        if (againstBy === 'Quotation' || againstBy === 'Proforma') {
            loadAgainstByValues(pick(m, 'AgainstByValue', 'againstByValue'));
        } else {
            $('#AgainstByValue').html('<option value="">N/A</option>').prop('disabled', true);
        }
    }

    // ============================================================
    // COMPANY
    // ============================================================

    function getCompanyDetails(cname) {

        $.ajax({
            url: pfUrls.getCompany,
            type: 'GET',
            data: { cname: cname },

            success: function (data) {

                if (!data) {
                    showToast('Company details not found.', 'warning');
                    clearCompanyDetails();
                    return;
                }

                $('#gstin').val(data.gstIn || '');

                if (data.gstIn && data.gstIn.trim() !== '') {
                    $('#billingStateCode').val(data.gstIn.substring(0, 2));
                }

                $('#address').val(data.address || '');
                $('#Location').val(data.location || '');
                $('#companyCode').val(data.companyCode || '');
                $('#billingState').val(data.state || '');
                $('#billingPincode').val(data.pinCode || '');

                applyGSTBasedOnCompany();
            },

            error: function () {
                showToast('Error while loading company details.', 'error');
            }
        });
    }

    function searchCompany(input) {

        const text = input.value.trim();
        const query = text.toLowerCase();
        const dropdown = document.getElementById('companyDropdown');

        // typed something different from the chosen company -> selection is no longer valid
        if (text !== selectedCompany) {
            const hadSelection = !!selectedCompany;
            selectedCompany = '';
            if (hadSelection) clearCompanyDetails();
        }

        if (!query) {
            dropdown.style.display = 'none';
            dropdown.innerHTML = '';
            return;
        }

        const matches = companyNames
            .filter(function (n) { return (n || '').toLowerCase().indexOf(query) !== -1; })
            .slice(0, 50);

        renderCompanyDropdown(matches, input, dropdown);
    }

    function renderCompanyDropdown(names, input, dropdown) {

        dropdown.innerHTML = '';

        if (!names.length) {
            dropdown.innerHTML = '<li style="padding:8px 12px;color:#999;">No companies found.</li>';
            dropdown.style.display = 'block';
            return;
        }

        const regex = new RegExp('(' + escapeRegex(input.value.trim()) + ')', 'gi');

        names.forEach(function (name, index) {

            const li = document.createElement('li');
            li.setAttribute('data-index', index);
            li.style.cssText = 'padding:8px 12px;cursor:pointer;border-bottom:1px solid #f0f0f0;font-size:12px;';
            li.innerHTML = escapeHtml(name).replace(regex, '<strong>$1</strong>');

            li.addEventListener('mouseenter', function () {
                clearHighlight(dropdown);
                li.style.background = '#f0f7ff';
                li.classList.add('highlighted');
            });

            li.addEventListener('mousedown', function (e) {
                e.preventDefault();
                selectCompany(name, input, dropdown);
            });

            dropdown.appendChild(li);
        });

        dropdown.style.display = 'block';
    }

    function selectCompany(name, input, dropdown) {

        input.value = name;
        selectedCompany = name;

        dropdown.style.display = 'none';
        dropdown.innerHTML = '';

        getCompanyDetails(name);

        // keep Quotation / Proforma list in sync with the company
        if (!isEdit && $('#AgainstBy').val() !== 'Direct') loadAgainstByValues();
    }

    function clearCompanyDetails() {

        ['#gstin', '#address', '#Location', '#billingState', '#billingPincode', '#billingStateCode']
            .forEach(function (id) { $(id).val(''); });

        $('.invoice-item-row').each(function () {
            const cgst = this.querySelector('.cgst-rate');
            const sgst = this.querySelector('.sgst-rate');
            const igst = this.querySelector('.igst-rate');

            if (cgst) { cgst.value = 0; cgst.disabled = false; }
            if (sgst) { sgst.value = 0; sgst.disabled = false; }
            if (igst) { igst.value = 0; igst.disabled = false; }

            if (cgst) calculateRow(cgst);
        });

        calculateAll();
    }

    // ============================================================
    // AGAINST BY  (Quotation / Proforma)
    // ============================================================

    function loadAgainstByValues(restoreValue) {

        const selected = $('#AgainstBy').val();
        const companyName = selectedCompany;
        const $val = $('#AgainstByValue');

        $val.html('<option value="">-- Select --</option>').prop('disabled', true);

        if (selected !== 'Quotation' && selected !== 'Proforma') {
            $val.html('<option value="">N/A</option>').prop('disabled', true);
            return;
        }

        if (!companyName) {
            showToast('Please select a Company first.', 'warning');
            $('#AgainstBy').val('Direct');
            $val.html('<option value="">N/A</option>').prop('disabled', true);
            return;
        }

        $val.html('<option value="">Loading...</option>');

        $.ajax({
            url: pfUrls.getQuotations,
            type: 'GET',
            data: { companyName: companyName, type: selected },

            success: function (response) {

                $val.html('<option value="">-- Select --</option>');

                if (!response || response.length === 0) {
                    $val.html('<option value="">No records found</option>');
                    return;
                }

                response.forEach(function (item) {
                    $val.append($('<option>').val(item.id).text(item.displayText));
                });

                if (restoreValue !== undefined && restoreValue !== null && restoreValue !== '') {
                    $val.val(String(restoreValue));
                }

                // edit: value is locked (rows already loaded); create: user can pick
                $val.prop('disabled', isEdit);
            },

            error: function () {
                $val.html('<option value="">No data</option>');
            }
        });
    }

    function loadAgainstByDetails() {

        if (isEdit) return;   // never overwrite saved rows in edit

        const selectedId = $('#AgainstByValue').val();
        const type = $('#AgainstBy').val();

        if (!selectedId) return;

        $.ajax({
            url: pfUrls.getQuotationDetails,
            type: 'GET',
            data: { id: selectedId, type: type },

            beforeSend: function () { $('#AgainstByValue').prop('disabled', true); },

            success: function (response) {
                if (!response) {
                    showToast('No data found.', 'warning');
                    return;
                }
                bindQuotationProformaData(response);
            },

            error: function () { showToast('Error loading details.', 'error'); },

            complete: function () { $('#AgainstByValue').prop('disabled', false); }
        });
    }

    // ============================================================
    // DATE
    // ============================================================

    function setTodayDate() {
        const d = new Date();
        const date = d.getFullYear() + '-' +
            String(d.getMonth() + 1).padStart(2, '0') + '-' +
            String(d.getDate()).padStart(2, '0');
        $('#invoiceDate').val(date);
    }

    function formatChequeDate(input) {

        let value = $(input).val().replace(/\D/g, '').substring(0, 8);

        if (value.length > 4) value = value.substring(0, 4) + '-' + value.substring(4);
        if (value.length > 7) value = value.substring(0, 7) + '-' + value.substring(7);

        $(input).val(value);

        if (value.length === 10) validateChequeDate(input);
        else $(input).removeClass('is-invalid');
    }

    function validateChequeDate(input) {

        const value = ($(input).val() || '').trim();

        if (!value) { $(input).removeClass('is-invalid'); return true; }

        const regex = /^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$/;

        if (!regex.test(value)) {
            $(input).addClass('is-invalid');
            showToast('Invalid date. Please use yyyy-MM-dd.', 'error');
            return false;
        }

        const date = new Date(value + 'T00:00:00');
        const ok = !isNaN(date.getTime()) &&
            date.getFullYear() === parseInt(value.substring(0, 4)) &&
            date.getMonth() + 1 === parseInt(value.substring(5, 7)) &&
            date.getDate() === parseInt(value.substring(8, 10));

        if (!ok) {
            $(input).addClass('is-invalid');
            showToast('Invalid date.', 'error');
            return false;
        }

        $(input).removeClass('is-invalid');
        return true;
    }

    // ============================================================
    // INVOICE ROW
    // ============================================================

    function addInvoiceRow() {

        rowCounter++;

        const row = document.createElement('tr');
        row.className = 'invoice-item-row';
        row.dataset.detailId = 0;

        row.innerHTML = `
            <td>
                <input type="text" value="${rowCounter}" readonly class="serial-number" />
            </td>

            <td class="service-desc-cell" colspan="2">
                <div class="sd-top">
                    <div class="sd-service">
                        <div class="service-wrapper">
                            <input type="text" placeholder="Search service..." class="service"
                                   autocomplete="off" />
                            <input type="hidden" class="service-id" />
                            <input type="hidden" class="service-name-hidden" />
                            <ul class="service-dropdown"></ul>
                        </div>
                    </div>
                    <div class="sd-sac">
                        <input type="text" class="sac-code" placeholder="SAC Code" />
                    </div>
                </div>
                <div class="sd-bottom">
                    <textarea class="description" placeholder="Enter service description..."></textarea>
                </div>
            </td>

            <td>
                <select class="validTill">
                    <option value="1 Year">1 Year</option>
                    <option value="2 Year">2 Year</option>
                    <option value="3 Year">3 Year</option>
                    <option value="4 Year">4 Year</option>
                    <option value="5 Year">5 Year</option>
                </select>
            </td>

            <td><input type="number" class="rate" min="0" value="0" /></td>

            <td><input type="number" class="taxable-value" value="0" readonly /></td>

            <td>
                <div class="tax-pair">
                    <input type="number" class="cgst-rate" min="0" value="0" placeholder="%" />
                    <input type="text" class="cgst-amount" value="0" readonly />
                </div>
            </td>

            <td>
                <div class="tax-pair">
                    <input type="number" class="sgst-rate" min="0" value="0" placeholder="%" />
                    <input type="text" class="sgst-amount" value="0" readonly />
                </div>
            </td>

            <td>
                <div class="tax-pair">
                    <input type="number" class="igst-rate" min="0" value="0" placeholder="%" />
                    <input type="text" class="igst-amount" value="0" readonly />
                </div>
            </td>

            <td><input type="number" class="row-total" value="0" readonly /></td>

            <td>
                <button type="button" class="btn btn-danger btn-sm remove-row" style="padding:1px 6px;">×</button>
            </td>
        `;

        document.getElementById('invoiceItems').appendChild(row);

        refreshSerialNumbers();
        applyGSTBasedOnCompany();
        calculateAll();
        $('#err-items').text('');

        return row;
    }

    function removeRow(btn) {

        const rows = document.querySelectorAll('.invoice-item-row');

        if (rows.length <= 1) {
            showToast('At least one item row is required.', 'warning');
            return;
        }

        showConfirmToast('Remove this row?', function () {
            btn.closest('tr').remove();
            refreshSerialNumbers();
            calculateAll();
        });
    }

    function refreshSerialNumbers() {
        document.querySelectorAll('.invoice-item-row').forEach(function (row, index) {
            row.querySelector('.serial-number').value = index + 1;
        });
    }

    // Fill a row from a saved / imported item, then recalculate it
    function setRowData(row, item) {

        if (!row || !item) return;

        const serviceName = pick(item, 'serviceName', 'ServiceName') || '';

        row.querySelector('.service').value = serviceName;
        row.querySelector('.service-name-hidden').value = serviceName;
        row.querySelector('.service-id').value = pick(item, 'serviceId', 'ServiceId') || '';

        row.querySelector('.sac-code').value = pick(item, 'saccode', 'sacCode', 'SacCode') || '';
        row.querySelector('.description').value =
            pick(item, 'productdescription', 'productDescription', 'ProductDescription', 'description') || '';

        const till = pick(item, 'serviceTill', 'ServiceTill') || '1 Year';
        const sel = row.querySelector('.validTill');
        sel.value = [...sel.options].some(o => o.value === till) ? till : sel.options[0].value;

        row.querySelector('.rate').value = parseFloat(pick(item, 'rate', 'Rate')) || 0;

        row.querySelector('.cgst-rate').value = parseFloat(pick(item, 'cgstrate', 'cgstRate', 'CgstRate')) || 0;
        row.querySelector('.sgst-rate').value = parseFloat(pick(item, 'sgstrate', 'sgstRate', 'SgstRate')) || 0;
        row.querySelector('.igst-rate').value = parseFloat(pick(item, 'igstrate', 'igstRate', 'IgstRate')) || 0;

        applyTaxState(row);
        calculateRow(row.querySelector('.rate'));
    }

    // ============================================================
    // LOAD EXISTING (EDIT)
    // ============================================================

    function loadExistingDetails() {

        $('#invoiceItems').empty();
        rowCounter = 0;

        const details = pfCfg.details || [];

        if (!details.length) {
            addInvoiceRow();
            return;
        }

        skipGstAuto = true;

        details.forEach(function (item) {
            const row = addInvoiceRow();
            row.dataset.detailId = pick(item, 'id', 'Id') || 0;
            setRowData(row, item);
        });

        skipGstAuto = false;

        refreshSerialNumbers();
        calculateAll();
    }

    function loadExistingBank() {

        const list = pfCfg.bankDetails || [];

        list.forEach(function (b) {
            appendBankRow({
                mode: pick(b, 'mode', 'Mode') || '',
                bankName: pick(b, 'bankName', 'BankName') || '',
                chequeNo: pick(b, 'chequeNo', 'ChequeNo', 'transactionNo', 'TransactionNo') || '',
                date: String(pick(b, 'date', 'Date', 'transactionDate', 'TransactionDate') || '').substring(0, 10),
                amount: parseFloat(pick(b, 'amount', 'Amount')) || 0,
                taxinvoiceid: String(pick(b, 'taxinvoiceid', 'taxInvoiceId', 'TaxInvoiceid')) || '0'
            });
        });

        if (list.length) $('#divtable').show();
    }

    // ============================================================
    // TAX
    // ============================================================

    function applyTaxState(row) {

        const cgst = row.querySelector('.cgst-rate');
        const sgst = row.querySelector('.sgst-rate');
        const igst = row.querySelector('.igst-rate');

        const c = parseFloat(cgst.value) || 0;
        const s = parseFloat(sgst.value) || 0;
        const i = parseFloat(igst.value) || 0;

        if (i > 0) {
            cgst.disabled = true; sgst.disabled = true; igst.disabled = false;
        } else if (c > 0 || s > 0) {
            cgst.disabled = false; sgst.disabled = false; igst.disabled = true;
        } else {
            cgst.disabled = false; sgst.disabled = false; igst.disabled = false;
        }
    }

    function taxChanged(input, taxType) {

        const row = input.closest('tr');
        if (!row) return;

        const cgst = row.querySelector('.cgst-rate');
        const sgst = row.querySelector('.sgst-rate');
        const igst = row.querySelector('.igst-rate');

        let value = parseFloat(input.value) || 0;

        if (value < 0) { input.value = 0; value = 0; }

        if (taxType === 'CGST' || taxType === 'SGST') {
            if (value > 0) {
                igst.value = 0;
                igst.disabled = true;
            } else if ((parseFloat(cgst.value) || 0) === 0 && (parseFloat(sgst.value) || 0) === 0) {
                igst.disabled = false;
            }
        }

        if (taxType === 'IGST') {
            if (value > 0) {
                cgst.value = 0; sgst.value = 0;
                cgst.disabled = true; sgst.disabled = true;
            } else {
                cgst.disabled = false; sgst.disabled = false;
            }
        }

        calculateRow(input);
    }

    // ============================================================
    // CALCULATIONS
    // ============================================================

    function calculateRow(element) {

        const row = element.closest('tr');
        if (!row) return;

        const rate = parseFloat(row.querySelector('.rate').value) || 0;
        const taxable = rate;

        const cgstRate = parseFloat(row.querySelector('.cgst-rate').value) || 0;
        const sgstRate = parseFloat(row.querySelector('.sgst-rate').value) || 0;
        const igstRate = parseFloat(row.querySelector('.igst-rate').value) || 0;

        const cgstAmount = taxable * cgstRate / 100;
        const sgstAmount = taxable * sgstRate / 100;
        const igstAmount = taxable * igstRate / 100;

        row.querySelector('.taxable-value').value = taxable.toFixed(2);
        row.querySelector('.cgst-amount').value = cgstAmount.toFixed(2);
        row.querySelector('.sgst-amount').value = sgstAmount.toFixed(2);
        row.querySelector('.igst-amount').value = igstAmount.toFixed(2);
        row.querySelector('.row-total').value = (taxable + cgstAmount + sgstAmount + igstAmount).toFixed(2);

        calculateAll();
    }

    function getBankTotal() {

        let total = 0;

        $('#tblBankDetail tbody tr').each(function () {
            total += parseFloat($(this).find('.bank-amount').val()) || 0;
        });

        return total;
    }

    function calculateAll() {

        let totalTaxable = 0, totalCGST = 0, totalSGST = 0, totalIGST = 0, grandTotal = 0;

        document.querySelectorAll('.invoice-item-row').forEach(function (row) {
            totalTaxable += getRowNumber(row, '.taxable-value');
            totalCGST += getRowNumber(row, '.cgst-amount');
            totalSGST += getRowNumber(row, '.sgst-amount');
            totalIGST += getRowNumber(row, '.igst-amount');
            grandTotal += getRowNumber(row, '.row-total');
        });

        const totalGST = totalCGST + totalSGST + totalIGST;

        // Invoice totals
        $('#totalTaxable').val(totalTaxable.toFixed(2));
        $('#totalCGST').val(totalCGST.toFixed(2));
        $('#totalSGST').val(totalSGST.toFixed(2));
        $('#totalIGST').val(totalIGST.toFixed(2));
        $('#grandTotalTable').val(grandTotal.toFixed(2));

        // Payment totals
        $('#txtTotalDealBasicAmount').val(totalTaxable.toFixed(2));
        $('#txtTotalDealGSTAmount').val(totalGST.toFixed(2));

        // Received (bank rows = basic received)
        const bankTotal = getBankTotal();
        $('#txtBasicAmountReceived').val(bankTotal.toFixed(2));

        const gstReceived = getNumber('txtGSTAmountReceived');

        // Balances
        $('#txtBalanceBasicAmount').val(Math.max(totalTaxable - bankTotal, 0).toFixed(2));
        $('#txtBalanceGSTAmount').val(Math.max(totalGST - gstReceived, 0).toFixed(2));

        // TDS
        const tdsPercentage = getNumber('txtTdsAmount');
        const tdsAmount = totalTaxable * tdsPercentage / 100;
        $('#txtTDSAmount').val(tdsAmount.toFixed(2));

        // Final balance
        const finalBalance = grandTotal - bankTotal - gstReceived - tdsAmount;
        $('#txtTotalAmountBalance').val(Math.max(finalBalance, 0).toFixed(2));

        updateAmountInWords();
    }

    // ============================================================
    // GST BASED ON COMPANY
    // ============================================================

    function applyGSTBasedOnCompany() {

        if (skipGstAuto) return;

        const gstin = ($('#gstin').val() || '').trim();
        if (gstin.length < 2) return;

        const customerStateCode = gstin.substring(0, 2);
        const companyStateCode = '27';   // Maharashtra

        $('.invoice-item-row').each(function () {

            const cgst = this.querySelector('.cgst-rate');
            const sgst = this.querySelector('.sgst-rate');
            const igst = this.querySelector('.igst-rate');

            if (!cgst || !sgst || !igst) return;

            if (customerStateCode === companyStateCode) {
                cgst.value = '9'; sgst.value = '9'; igst.value = '0';
                cgst.disabled = false; sgst.disabled = false; igst.disabled = true;
            } else {
                cgst.value = '0'; sgst.value = '0'; igst.value = '18';
                cgst.disabled = true; sgst.disabled = true; igst.disabled = false;
            }

            calculateRow(cgst);
        });

        calculateAll();
    }

    // ============================================================
    // BANK DETAILS
    // ============================================================

    function appendBankRow(b) {

        const parsedId = parseInt(b.taxinvoiceid, 10);
        const hasTaxInvoice = Number.isInteger(parsedId) && parsedId > 0;
        const taxInvoiceId = hasTaxInvoice ? parsedId : '';

        const firstCell = hasTaxInvoice
            ? `<button type="button" class="btn-pdf-view" data-id="${taxInvoiceId}" title="View Tax Invoice PDF">
               <i class="fa fa-file-pdf"></i><span>PDF</span>
           </button>`
            : `<input type="checkbox" class="bank-select">`;

        const row = `
        <tr>
            <td class="text-center">
                <input type="hidden" class="tax-invoice-id" value="${taxInvoiceId}">
                ${firstCell}
            </td>
            <td><input type="text" class="form-control mode" value="${escapeHtml(b.mode)}" readonly></td>
            <td><input type="text" class="form-control bank-name" value="${escapeHtml(b.bankName)}" readonly></td>
            <td><input type="text" class="form-control bank-cheque-no" value="${escapeHtml(b.chequeNo)}" readonly></td>
            <td><input type="text" class="form-control bank-date" value="${escapeHtml(b.date)}" readonly></td>
            <td><input type="text" class="form-control bank-amount text-right" value="${Number(b.amount).toFixed(2)}" readonly></td>
        </tr>
    `;

        $('#tblBankDetail tbody').append(row);
    }

 
    function openTaxInvoicePdf(id) {

        // open the tab first (inside the click) so the popup blocker allows it
        const win = window.open('', '_blank');

        $.ajax({
            url: pfUrls.taxInvoicePdf,
            type: 'POST',
            data: { id: id },
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },

            success: function (res) {
                if (res && res.success && res.url) {
                    if (win) win.location.href = res.url;
                    else window.open(res.url, '_blank');
                } else {
                    if (win) win.close();
                    showToast('Unable to open tax invoice PDF.', 'error');
                }
            },

            error: function () {
                if (win) win.close();
                showToast('Error while opening tax invoice PDF.', 'error');
            }
        });
    }
    function addBankDetail() {

        const mode = ($('#ddlPaymentMode').val() || '').trim();
        const bankName = ($('#txtBankName').val() || '').trim();
        const chequeNo = ($('#txtChequeNo').val() || '').trim();
        const chequeDate = ($('#txtChequedate').val() || '').trim();
        const amount = parseFloat($('#txtAmount').val()) || 0;

        if (!mode) {
            showToast('Please select Payment Mode.', 'warning');
            $('#ddlPaymentMode').focus();
            return;
        }

        if (!bankName) {
            showToast('Please enter Bank Name.', 'warning');
            $('#txtBankName').focus();
            return;
        }

        if (!chequeNo) {
            showToast('Please enter Cheque / Transaction No.', 'warning');
            $('#txtChequeNo').focus();
            return;
        }

        if (!chequeDate) {
            showToast('Please select Date.', 'warning');
            $('#txtChequedate').focus();
            return;
        }

        if (!validateChequeDate(document.getElementById('txtChequedate'))) return;

        if (amount <= 0) {
            showToast('Please enter a valid Amount.', 'warning');
            $('#txtAmount').focus();
            return;
        }

        // Duplicate cheque / transaction no.
        let duplicate = false;

        $('#tblBankDetail tbody tr').each(function () {
            const existing = ($(this).find('.bank-cheque-no').val() || '').trim();
            if (existing && existing.toLowerCase() === chequeNo.toLowerCase()) {
                duplicate = true;
                return false;
            }
        });

        if (duplicate) {
            showToast('This cheque / transaction number is already added.', 'error');
            $('#txtChequeNo').focus();
            return;
        }

        appendBankRow({ mode: mode, bankName: bankName, chequeNo: chequeNo, date: chequeDate, amount: amount });

        $('#divtable').show();

        $('#ddlPaymentMode').val('');
        $('#txtBankName').val('');
        $('#txtChequeNo').val('');
        $('#txtChequedate').val('');
        $('#txtAmount').val('');

        calculateAll();
    }

    function deleteBankDetails() {

        const selectedRows = $('#tblBankDetail tbody .bank-select:checked');

        if (selectedRows.length === 0) {
            showToast('Please select at least one bank detail.', 'warning');
            return;
        }

        showConfirmToast('Are you sure you want to delete selected bank details?', function () {

            selectedRows.each(function () { $(this).closest('tr').remove(); });

            if ($('#tblBankDetail tbody tr').length === 0) $('#divtable').hide();

            calculateAll();
        });
    }

    function validateBankDetails() {

        const invoiceTotal = getNumber('grandTotalTable');

        if (getBankTotal() > invoiceTotal) {
            showToast('Bank received amount cannot be greater than invoice amount.', 'error');
            return false;
        }

        return true;
    }

    // ============================================================
    // SERVICE SEARCH
    // ============================================================

    function searchService(input) {

        const query = input.value.trim();
        const wrapper = input.closest('.service-wrapper');
        const dropdown = wrapper.querySelector('.service-dropdown');

        wrapper.querySelector('.service-id').value = '';

        if (query.length < 2) {
            dropdown.style.display = 'none';
            dropdown.innerHTML = '';
            return;
        }

        dropdown.style.display = 'block';
        dropdown.innerHTML = '<li style="padding:8px 12px;color:#999;">Searching...</li>';

        clearTimeout(searchTimer);

        searchTimer = setTimeout(function () {
            fetchServices(query, input, dropdown);
        }, 300);
    }

    function fetchServices(query, input, dropdown) {

        $.ajax({
            url: pfUrls.searchServices,
            type: 'GET',
            data: { q: query },

            success: function (response) { renderServiceDropdown(response, input, dropdown); },

            error: function () {
                dropdown.innerHTML = '<li style="padding:8px 12px;color:red;">Error fetching services.</li>';
            }
        });
    }

    function renderServiceDropdown(services, input, dropdown) {

        dropdown.innerHTML = '';

        if (!services || services.length === 0) {
            dropdown.innerHTML = '<li style="padding:8px 12px;color:#999;">No services found.</li>';
            dropdown.style.display = 'block';
            return;
        }

        services.forEach(function (svc, index) {

            const li = document.createElement('li');
            li.setAttribute('data-index', index);
            li.style.cssText = 'padding:8px 12px;cursor:pointer;border-bottom:1px solid #f0f0f0;font-size:11px;';

            const regex = new RegExp('(' + escapeRegex(input.value.trim()) + ')', 'gi');
            const highlighted = escapeHtml(svc.serviceName || '').replace(regex, '<strong>$1</strong>');

            li.innerHTML = `
                <div style="font-weight:500;">${highlighted}</div>
                ${svc.saccode ? `<div style="font-size:9px;color:#888;">SAC: ${escapeHtml(svc.saccode)}</div>` : ''}
            `;

            li.addEventListener('mouseenter', function () {
                clearHighlight(dropdown);
                li.style.background = '#f0f7ff';
                li.classList.add('highlighted');
            });

            li.addEventListener('mousedown', function (e) {
                e.preventDefault();
                selectService(svc, input);
            });

            dropdown.appendChild(li);
        });

        dropdown.style.display = 'block';
    }

    function selectService(svc, input) {

        const row = input.closest('tr');
        const wrapper = input.closest('.service-wrapper');

        input.value = svc.serviceName || '';
        wrapper.querySelector('.service-id').value = svc.serviceId || '';
        wrapper.querySelector('.service-name-hidden').value = svc.serviceName || '';

        row.querySelector('.sac-code').value = svc.saccode || '998314';
        row.querySelector('.rate').value = parseFloat(svc.rate) || 0;

        const dropdown = wrapper.querySelector('.service-dropdown');
        dropdown.style.display = 'none';
        dropdown.innerHTML = '';

        calculateRow(row.querySelector('.rate'));
        applyGSTBasedOnCompany();
        calculateAll();
    }

    function handleServiceKeydown(e, input) {

        const dropdown = input.closest('.service-wrapper').querySelector('.service-dropdown');
        const items = dropdown.querySelectorAll('li[data-index]');

        if (!items.length) return;

        const current = dropdown.querySelector('.highlighted');
        let index = current ? parseInt(current.getAttribute('data-index')) : -1;

        if (e.key === 'ArrowDown') {
            e.preventDefault();
            index = Math.min(index + 1, items.length - 1);
            setHighlight(items, index);
        }
        else if (e.key === 'ArrowUp') {
            e.preventDefault();
            index = Math.max(index - 1, 0);
            setHighlight(items, index);
        }
        else if (e.key === 'Enter') {
            e.preventDefault();
            if (current) {
                current.dispatchEvent(new MouseEvent('mousedown', { bubbles: true, cancelable: true }));
            }
        }
        else if (e.key === 'Escape') {
            dropdown.style.display = 'none';
        }
    }

    function setHighlight(items, index) {

        items.forEach(function (li) {
            li.style.background = '';
            li.classList.remove('highlighted');
        });

        if (!items[index]) return;

        items[index].style.background = '#f0f7ff';
        items[index].classList.add('highlighted');
        items[index].scrollIntoView({ block: 'nearest' });
    }

    function clearHighlight(dropdown) {
        dropdown.querySelectorAll('li').forEach(function (li) {
            li.style.background = '';
            li.classList.remove('highlighted');
        });
    }

    // ============================================================
    // QUOTATION / PROFORMA BIND (create only)
    // ============================================================

    function bindQuotationProformaData(data) {

        $('#invoiceItems').empty();
        rowCounter = 0;

        if (!data || !data.details || data.details.length === 0) {
            addInvoiceRow();
            return;
        }

        skipGstAuto = true;

        data.details.forEach(function (item) {
            const row = addInvoiceRow();
            setRowData(row, item);
        });

        skipGstAuto = false;

        refreshSerialNumbers();

        // re-apply company based GST (does nothing if no GSTIN yet)
        applyGSTBasedOnCompany();
        calculateAll();
    }

    // ============================================================
    // NUMBER TO WORDS
    // ============================================================

    function numberToWordsIndian(amount) {

        amount = parseFloat(amount) || 0;

        if (amount === 0) return 'Zero Rupees Only';

        amount = Math.round(amount * 100) / 100;

        const ones = ['', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine', 'Ten',
            'Eleven', 'Twelve', 'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen', 'Seventeen', 'Eighteen', 'Nineteen'];

        const tens = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];

        function belowThousand(num) {
            let result = '';

            if (num >= 100) {
                result += ones[Math.floor(num / 100)] + ' Hundred ';
                num %= 100;
            }
            if (num >= 20) {
                result += tens[Math.floor(num / 10)] + ' ';
                num %= 10;
            }
            if (num > 0) result += ones[num] + ' ';

            return result.trim();
        }

        function convertIndian(num) {
            num = Math.floor(num);

            if (num < 1000) return belowThousand(num);
            if (num < 100000) return convertIndian(Math.floor(num / 1000)) + ' Thousand ' + belowThousand(num % 1000);
            if (num < 10000000) return convertIndian(Math.floor(num / 100000)) + ' Lakh ' + convertIndian(num % 100000);

            return convertIndian(Math.floor(num / 10000000)) + ' Crore ' + convertIndian(num % 10000000);
        }

        const rupees = Math.floor(amount);
        const paise = Math.round((amount - rupees) * 100);

        let result = convertIndian(rupees).replace(/\s+/g, ' ').trim() + ' Rupees';

        if (paise > 0) result += ' and ' + convertIndian(paise) + ' Paise';

        return result + ' Only';
    }

    function updateAmountInWords() {

        // #amountInWords is optional – only filled if the element exists
        if ($('#amountInWords').length) {
            $('#amountInWords').val(numberToWordsIndian(getNumber('txtTotalAmountBalance')));
        }
    }

    // ============================================================
    // VALIDATION
    // ============================================================

    function validateInvoice() {

        let valid = true;

        $('.field-error').removeClass('field-error');
        $('.err-msg').text('');

        if (!$('#invoiceDate').val()) {
            $('#invoiceDate').addClass('field-error');
            $('#err-invoiceDate').text('Invoice Date is required.');
            valid = false;
        }

        if (!selectedCompany) {
            $('#companyName').addClass('field-error');
            $('#err-companyName').text('Please search and select a company from the list.');
            valid = false;
        }

        let itemsValid = true;

        $('.invoice-item-row').each(function () {

            const service = $(this).find('.service');
            const rate = $(this).find('.rate');

            if (!service.val().trim()) {
                service.addClass('field-error');
                itemsValid = false;
            }

            if ((parseFloat(rate.val()) || 0) <= 0) {
                rate.addClass('field-error');
                itemsValid = false;
            }
        });

        if (!itemsValid) {
            $('#err-items').text('All rows need a service name and rate greater than 0.');
            valid = false;
        }

        if (getNumber('grandTotalTable') <= 0) {
            $('#err-items').text('Grand total must be greater than zero.');
            valid = false;
        }

        if (!validateBankDetails()) valid = false;

        if (!valid) {
            const firstError = document.querySelector('.field-error');
            if (firstError) firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }

        return valid;
    }

    // ============================================================
    // SAVE / UPDATE
    // ============================================================

    function saveInvoice() {

        calculateAll();

        if (!validateInvoice()) return;

        // ---- Totals ----
        const totalTaxable = getNumber('totalTaxable');
        const totalCGST = getNumber('totalCGST');
        const totalSGST = getNumber('totalSGST');
        const totalIGST = getNumber('totalIGST');
        const totalGST = totalCGST + totalSGST + totalIGST;
        const grandTotal = getNumber('grandTotalTable');

        // ---- Payment ----
        const basicReceived = getNumber('txtBasicAmountReceived');
        const gstReceived = getNumber('txtGSTAmountReceived');
        const tdsPercentage = getNumber('txtTdsAmount');
        const tdsAmount = getNumber('txtTDSAmount');
        const balanceBasic = getNumber('txtBalanceBasicAmount');
        const balanceGST = getNumber('txtBalanceGSTAmount');

        const totalAmountReceived = basicReceived + gstReceived;
        const pendingAmountBeforeTDS = balanceBasic + balanceGST;
        const bankAmountReceived = getBankTotal();
        const finalPendingAmount = Math.max(pendingAmountBeforeTDS - tdsAmount, 0);

        // ---- Bank rows ----
        const bankDetails = [];

        $('#tblBankDetail tbody tr').each(function () {

            const taxInvoiceIdVal = ($(this).find('.tax-invoice-id').val() || '').trim();
            const mode = $(this).find('.mode').val() || '';
            const bankName = $(this).find('.bank-name').val() || '';
            const chequeNo = $(this).find('.bank-cheque-no').val() || '';
            const date = $(this).find('.bank-date').val() || '';
            const amount = parseFloat($(this).find('.bank-amount').val()) || 0;

            if (mode || bankName || chequeNo || date || amount > 0) {
                bankDetails.push({
                    taxInvoiceId: taxInvoiceIdVal !== '' ? taxInvoiceIdVal : null,
                    mode: mode.trim() || null,
                    bankName: bankName.trim() || null,
                    chequeNo: chequeNo.trim() || null,
                    date: date || null,
                    amount: Number(amount.toFixed(2))
                });
            }
        });


        // ---- Main ----
        const main = {

            Id: isEdit ? String(invoiceId) : null,
            invoiceno: getValue('invoiceNo') || null,
            invoicedate: getValue('invoiceDate') || null,
            reversecharge: getValue('reverseCharge') || null,
            InvoiceType: getValue('InvoiceType') || null,
            AgainstBy: getValue('AgainstBy') || null,
            AgainstByValue: getValue('AgainstByValue') || null,

            // company
            companyName: getValue('companyName') || null,
            companyCode: getValue('companyCode') || null,
            gstIn: getValue('gstin') || null,
            Address: getValue('address') || null,
            Location: getValue('Location') || null,
            PinCode: getValue('billingPincode') || null,
            state: getValue('billingState') || null,
            statecode: getValue('billingStateCode') || null,


            // GST
            cgst: totalCGST > 0 ? 9 : 0,
            cgstamt: Number(totalCGST.toFixed(2)),
            sgst: totalSGST > 0 ? 9 : 0,
            sgstamt: Number(totalSGST.toFixed(2)),
            igst: totalIGST > 0 ? 18 : 0,
            igstamt: Number(totalIGST.toFixed(2)),
            gstonreversecharge: getValue('gstOnReverseCharge') || null,

            // totals
            totalqty: $('.invoice-item-row').length,
            totalrate: Number(totalTaxable.toFixed(2)),
            taxablevalue: Number(totalTaxable.toFixed(2)),
            totalamtbeforetax: Number(totalTaxable.toFixed(2)),
            totalamtaftertax: Number(grandTotal.toFixed(2)),
            total_tax_amount: Number(totalGST.toFixed(2)).toString(),
            amtinwords: numberToWordsIndian(finalPendingAmount),

            // payment summary
            TotalBasicAmount: Number(totalTaxable.toFixed(2)),
            TotalTaxableAmount: Number(totalTaxable.toFixed(2)),
            TotalCGSTAmount: Number(totalCGST.toFixed(2)),
            TotalSGSTAmount: Number(totalSGST.toFixed(2)),
            TotalIGSTAmount: Number(totalIGST.toFixed(2)),
            TotalGSTAmount: Number(totalGST.toFixed(2)),
            TotalInvoiceAmount: Number(grandTotal.toFixed(2)),
            BasicAmountReceived: Number(basicReceived.toFixed(2)),
            GSTAmountReceived: Number(gstReceived.toFixed(2)),
            TotalAmountReceived: Number(totalAmountReceived.toFixed(2)),
            PendingBasicAmount: Number(balanceBasic.toFixed(2)),
            PendingGSTAmount: Number(balanceGST.toFixed(2)),
            PendingAmountBeforeTDS: Number(pendingAmountBeforeTDS.toFixed(2)),
            TDSPercentage: Number(tdsPercentage.toFixed(2)),
            TDSAmount: Number(tdsAmount.toFixed(2)),
            BankAmountReceived: Number(bankAmountReceived.toFixed(2)),
            TotalTDSAmount: Number(tdsAmount.toFixed(2)),
            FinalPendingAmount: Number(finalPendingAmount.toFixed(2)),
            TotalAmountBalance: Number(finalPendingAmount.toFixed(2)),

            // service / session
            servicedescription: getValue('serviceDescription') || null,
            sessionname: getValue('sessionname') || null,
            NAME: getValue('NAME') || null,

            // billing
            BillingAddress: getValue('address') || null,
            BillingLocation: getValue('Location') || null,
            BillingGST: getValue('gstin') || null,
            BillingPincode: getValue('billingPincode') || null,
            BillingStatecode: getValue('billingStateCode') || null
        };

        // ---- Details ----
        const details = [];

        $('.invoice-item-row').each(function () {

            const row = this;

            const serviceName =
                row.querySelector('.service-name-hidden')?.value ||
                row.querySelector('.service')?.value || '';

            const serviceId = row.querySelector('.service-id')?.value || '';
            const description = row.querySelector('.description')?.value || '';
            const sacCode = row.querySelector('.sac-code')?.value || '';
            const serviceTill = row.querySelector('.validTill')?.value || '';

            const rate = getRowNumber(row, '.rate');
            const taxableValue = getRowNumber(row, '.taxable-value');

            details.push({

                id: isEdit ? (parseInt(row.dataset.detailId) || 0) : 0,
                invoiceid: isEdit ? invoiceId : 0,

                serviceName: serviceName.trim() || null,
                serviceId: serviceId.trim() || null,
                serviceTill: serviceTill.trim() || null,
                productdescription: description.trim() || null,
                saccode: sacCode.trim() || null,

                qty: 1,
                rate: Number(rate.toFixed(2)),
                amount: Number(taxableValue.toFixed(2)),
                taxablevalue: Number(taxableValue.toFixed(2)),

                cgstrate: Number(getRowNumber(row, '.cgst-rate').toFixed(2)),
                cgstamt: Number(getRowNumber(row, '.cgst-amount').toFixed(2)),
                sgstrate: Number(getRowNumber(row, '.sgst-rate').toFixed(2)),
                sgstamt: Number(getRowNumber(row, '.sgst-amount').toFixed(2)),
                igstrate: Number(getRowNumber(row, '.igst-rate').toFixed(2)),
                igstamt: Number(getRowNumber(row, '.igst-amount').toFixed(2)),

                total: Number(getRowNumber(row, '.row-total').toFixed(2))
            });
        });

        // ---- Payload ----
        const payload = {
            main: main,
            details: details,
            companies: [],
            BankDetails: bankDetails
        };

        console.log('Invoice Save Payload:', JSON.stringify(payload, null, 4));

        // ---- Send ----
        const btnText = isEdit ? 'Update' : 'Save';
        const $btn = $('#saveBtn');

        $btn.prop('disabled', true).text(isEdit ? 'Updating...' : 'Saving...');

        $.ajax({
            url: isEdit ? pfUrls.update : pfUrls.save,
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',         
            data: JSON.stringify(payload),
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (response) {

                console.log('Save Response:', response);

                if (response && response.success === true) {

                    showToast(
                        isEdit
                            ? 'Invoice updated successfully! Invoice No: ' + (response.invoiceNo || getValue('invoiceNo'))
                            : 'Invoice saved successfully!',
                        'success'
                    );


                    setTimeout(function () {
                        window.location.href = pfUrls.index;
                    }, 2000);
                }
                else {
                    showToast(
                        (isEdit ? 'Update failed: ' : 'Save failed: ') +
                        (response?.message || 'Unable to save invoice.'),
                        'error'
                    );
                }
            },

            error: function (xhr) {

                console.error('Save Invoice Error:', xhr.responseText);

                let message = 'Error saving invoice.';

                try {
                    const response = JSON.parse(xhr.responseText);
                    if (response.message) message = response.message;
                    if (response.errors) console.error('Validation Errors:', response.errors);
                }
                catch (e) {
                    console.error('Unable to parse server response.', e);
                }

                showToast(message, 'error');
            },

            complete: function () {
                $btn.prop('disabled', false).text(btnText);
            }
        });
    }

    // ============================================================
    // CLEAR / RESET
    // ============================================================

    function clearInvoice() {

        // Edit: discard changes by reloading the saved invoice
        if (isEdit) {
            showConfirmToast('Discard all changes and reload the saved invoice?', function () {
                window.location.reload();
            });
            return;
        }

        showConfirmToast('Are you sure you want to clear the invoice?', function () {

            $('#companyName').val('');
            selectedCompany = '';
            clearCompanyDetails();
            $('#reverseCharge').val('N');
            $('#Transaction').val('');
            $('#AgainstBy').val('Direct');

            $('#AgainstByValue').html('<option value="">N/A</option>').prop('disabled', true);

            ['#gstin', '#address', '#Location', '#billingState', '#billingPincode', '#billingStateCode',
                '#transNo', '#transDate', '#TransAmt']
                .forEach(function (id) { $(id).val(''); });

            ['#txtBankName', '#txtChequeNo', '#txtChequedate', '#txtAmount', '#txtGSTAmountReceived', '#txtTdsAmount']
                .forEach(function (id) { $(id).val(''); });

            $('#ddlPaymentMode').val('');
            $('#tblBankDetail tbody').empty();
            $('#divtable').hide();

            $('#invoiceItems').empty();
            rowCounter = 0;
            addInvoiceRow();

            setTodayDate();

            $('.field-error').removeClass('field-error');
            $('.err-msg').text('');

            calculateAll();
        });
    }


    function formatDateToDDMMYYYY(dateValue) {
        if (!dateValue) {
            return "";
        }

        var date = new Date(dateValue);

        if (isNaN(date.getTime())) {
            return "";
        }

        var day = String(date.getDate()).padStart(2, "0");
        var month = String(date.getMonth() + 1).padStart(2, "0");
        var year = date.getFullYear();

        return year + "-" + month + "-" + day;
    }
  

    return {
        init: function (config) {

            pfCfg = config || {};
            isEdit = !!pfCfg.isEdit;
            invoiceId = pfCfg.invoiceId || 0;
            pfUrls = pfCfg.urls || {};
            companyNames = pfCfg.companies || [];

            $(document).ready(function () {
                initializePage();
            });
        }
    };
}();
