import os

# Thư mục gốc chứa các project API (bạn chỉnh lại nếu khác)
root_dir = r"E:\C#\DOS"

# Danh sách các folder con cần tạo
folders_to_create = [
    "DTOs",
    os.path.join("Services", "Interfaces"),
    os.path.join("Repositories", "Interfaces"),
    "Profile",
    "Models"
]

for folder_name in os.listdir(root_dir):
    folder_path = os.path.join(root_dir, folder_name)

    # Chỉ xử lý folder có "API" trong tên
    if os.path.isdir(folder_path) and "API" in folder_name:
        print(f"📂 Đang xử lý: {folder_name}")

        for subfolder in folders_to_create:
            subfolder_path = os.path.join(folder_path, subfolder)
            os.makedirs(subfolder_path, exist_ok=True)

            # Thêm file .gitkeep để folder không rỗng
            placeholder = os.path.join(subfolder_path, ".gitkeep")
            if not os.path.exists(placeholder):
                with open(placeholder, "w", encoding="utf-8") as f:
                    f.write("")  # file rỗng
            print(f"   ✅ Đã tạo: {subfolder_path} (kèm .gitkeep)")

print("🎉 Hoàn tất!")
