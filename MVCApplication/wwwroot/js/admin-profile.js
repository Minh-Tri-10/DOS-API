(function () {
    const card = $('#profileCard');
    const btnEdit = $('#btnEdit');
    const btnSave = $('#btnSave');
    const btnCancel = $('#btnCancel');
    const btnChooseAvatar = $('#btnChooseAvatar');
    const avatarFile = $('#avatarFile');
    const preview = $('previewAvatar');
    const form = $('#profileForm');

    const editable = ['#inpFullName', '#inpEmail', '#inpPhone', '#btnChooseAvatar']
        .map(sel => $(sel))
        .filter(x => x.length);

    function setEditing(on) {
        card.attr('data-editing', on ? 'true' : 'false');
        editable.forEach(i => i.prop('disabled', !on));
        avatarFile.prop('disabled', !on);
        btnSave.toggleClass('hidden', !on);
        btnCancel.toggleClass('hidden', !on);
        btnEdit.text(on ? 'Đang chỉnh sửa…' : 'Edit');
        if (on) btnSave.prop('disabled', true);
    }

    // Nút Edit
    btnEdit.on('click', () => setEditing(true));

    // Nút Hủy
    btnCancel.on('click', function () {
        $.get('/AdminAccounts/Profile', html => {
            const newCard = $(html).find('#profileCard');
            card.fadeOut(80, function () {
                $(this).replaceWith(newCard);
                newCard.fadeIn(150);
                $.getScript('/js/admin-profile.js');
            });
        });
    });

    // Nút Lưu (AJAX)
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
                // ✅ Lưu lại header cũ
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

                    $.getScript('/js/admin-profile.js', function () {
                        if (hasError) {
                            const c = newCard;
                            c.attr('data-editing', 'true');
                            c.find('#inpFullName,#inpEmail,#inpPhone,#btnChooseAvatar')
                                .prop('disabled', false);
                            c.find('#btnSave,#btnCancel').removeClass('hidden');
                            c.find('#btnEdit').text('Đang chỉnh sửa…');

                            // ✅ Giữ lại header cũ
                            $('.avatar-btn img.avatar-img').attr('src', oldHeader.avatar);
                            $('.head-meta .name').text(oldHeader.name);
                            $('.head-meta .sub').html(oldHeader.emailSub);
                            return;
                        }

                        // ✅ Sau khi cập nhật thành công
                        const updatedName = newCard.find('#inpFullName').val()?.trim() || '';
                        const updatedEmail = newCard.find('#inpEmail').val()?.trim() || '';
                        const updatedAvatar = newCard.find('#previewAvatarSmall').attr('src');
                        const updatedRole = newCard.find('input[value="admin"],input[value="manager"]').val() || 'admin';

                        $('.avatar-btn img.avatar-img').attr('src', updatedAvatar);
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

    // Avatar
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

    // Cho phép lưu khi input thay đổi
    editable.forEach(i => {
        if (i.attr('type') !== 'file') {
            i.on('input', () => {
                if (card.attr('data-editing') === 'true')
                    btnSave.prop('disabled', false);
            });
        }
    });

    // Giữ chế độ Edit khi server trả về lỗi
    if (window.keepEditing === true) {
        setEditing(true);
    }
})();
