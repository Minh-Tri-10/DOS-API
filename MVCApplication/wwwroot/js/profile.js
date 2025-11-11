(function () {
    const card = $('#profileCard');
    const btnEdit = $('#btnEdit');
    const btnSave = $('#btnSave');
    const btnCancel = $('#btnCancel');
    const btnChooseAvatar = $('#btnChooseAvatar');
    const avatarFile = $('#avatarFile');
    const preview = $('#previewAvatar');
    const form = $('#profileForm');

    const editable = ['#inpFullName', '#inpEmail', '#inpPhone', '#btnChooseAvatar']
        .map(sel => $(sel))
        .filter(x => x.length);

    // === Toggle edit mode ===
    function setEditing(on) {
        card.attr('data-editing', on ? 'true' : 'false');
        editable.forEach(i => i.prop('disabled', !on));
        btnSave.toggleClass('hidden', !on);
        btnCancel.toggleClass('hidden', !on);
        btnEdit.text(on ? 'Đang chỉnh sửa...' : 'Edit');
        if (on) btnSave.prop('disabled', true);
    }

    // === Edit ===
    btnEdit.on('click', () => setEditing(true));

    // === Cancel ===
    btnCancel.on('click', function () {
        $.get('/Accounts/Profile', html => {
            const newCard = $(html).find('#profileCard');
            card.fadeOut(80, function () {
                $(this).replaceWith(newCard);
                newCard.fadeIn(150);
                $.getScript('/js/profile.js'); // bind lại sự kiện
            });
        });
    });

    // === Save ===
    btnSave.on('click', function (e) {
        e.preventDefault();
        const formData = new FormData(form[0]);
        btnSave.prop('disabled', true).text('Đang lưu...');

        $.ajax({
            url: form.attr('action'),
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (html) {
                const oldHeader = {
                    name: $('.head-meta .name').text().trim(),
                    emailSub: $('.head-meta .sub').html(),
                    avatar: $('.avatar-btn img.avatar-img').attr('src')
                };

                const newCard = $(html).find('#profileCard');
                const hasError = newCard.find('.text-danger')
                    .filter((i, el) => $(el).text().trim().length > 0)
                    .length > 0;

                card.fadeOut(80, function () {
                    $(this).replaceWith(newCard);
                    newCard.fadeIn(150);

                    $.getScript('/js/profile.js', function () {
                        if (hasError) {
                            const c = newCard;
                            c.attr('data-editing', 'true');
                            c.find('#inpFullName,#inpEmail,#inpPhone,#btnChooseAvatar')
                                .prop('disabled', false);
                            c.find('#btnSave,#btnCancel').removeClass('hidden');
                            c.find('#btnEdit').text('Đang chỉnh sửa...');
                        }

                        // ✅ Nếu có lỗi validation → giữ nguyên header cũ
                        if (hasError) {
                            $('.avatar-btn img.avatar-img').attr('src', oldHeader.avatar);
                            $('#userModal .avatar-lg').attr('src', oldHeader.avatar);
                            $('.head-meta .name').text(oldHeader.name);
                            $('.head-meta .sub').html(oldHeader.emailSub);
                            return;
                        }

                        // ✅ Sau khi cập nhật thành công → cập nhật header mới
                        const updatedName = newCard.find('#inpFullName').val()?.trim() || '';
                        const updatedEmail = newCard.find('#inpEmail').val()?.trim() || '';
                        const updatedAvatar = newCard.find('#previewAvatar').attr('src');
                        const updatedRole = newCard.find('input[value="customer"],input[value="admin"]').val() || 'customer';

                        $('.avatar-btn img.avatar-img').attr('src', updatedAvatar);
                        $('#userModal .avatar-lg').attr('src', updatedAvatar);
                        $('#userModal .fw-bold').text(updatedName || 'Người dùng');
                        $('#userModal .text-muted').text(`${updatedEmail} • ${updatedRole}`);
                        $('.head-meta .name').text(updatedName);
                        $('.head-meta .sub').html(`${updatedEmail} • <span>${updatedRole}</span>`);
                    });
                });
            },
            error: function (xhr) {
                alert('❌ Cập nhật thất bại. Vui lòng thử lại.');
                console.error(xhr.responseText);
            },
            complete: function () {
                btnSave.text('Lưu thay đổi').prop('disabled', false);
            }
        });
    });

    // === Avatar select ===
    btnChooseAvatar.on('click', () => avatarFile.click());
    avatarFile.on('change', function (e) {
        const file = e.target.files[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = ev => {
            preview.attr('src', ev.target.result);
            btnSave.prop('disabled', false);
        };
        reader.readAsDataURL(file);
    });

    // === Detect typing ===
    editable.forEach(i => {
        if (i.attr('type') !== 'file') {
            i.on('input', () => {
                if (card.attr('data-editing') === 'true')
                    btnSave.prop('disabled', false);
            });
        }
    });

    // === Keep edit mode if ViewBag.KeepEditing ===
    if (window.keepEditing === true) {
        setEditing(true);
    }
})();
