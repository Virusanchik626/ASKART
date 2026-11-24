import os
from pathlib import Path

def collect_files():
    """Собирает список файлов для обработки"""
    base_dir = Path.cwd()
    files_to_process = []
    
    # Папки для сканирования (включая подпапки)
    target_folders = ['Controllers', 'Data', 'DTOs', 'Hubs', 'Models']
    
    # 1. Файлы .cs из основной директории (не рекурсивно)
    for item in base_dir.iterdir():
        if item.is_file() and item.suffix == '.cs':
            files_to_process.append(item)
        # Файлы Dockerfile и docker-compose.yaml
        if item.is_file() and item.name in ('Dockerfile', 'docker-compose.yaml', 'docker-compose.yml'):
            files_to_process.append(item)
    
    # 2. Файлы .cs и .xaml.cs из указанных папок (рекурсивно)
    for folder_name in target_folders:
        folder_path = base_dir / folder_name
        if folder_path.exists() and folder_path.is_dir():
            for file_path in folder_path.rglob('*'):
                if file_path.is_file():
                    if file_path.suffix == '.cs' or file_path.name.endswith('.xaml.cs'):
                        files_to_process.append(file_path)
    
    # Удаляем дубликаты и сортируем
    files_to_process = sorted(set(files_to_process))
    return files_to_process


def write_files_content(files, output_file='all_api_files.txt'):
    """Записывает содержимое файлов в выходной файл"""
    base_dir = Path.cwd()
    
    with open(output_file, 'w', encoding='utf-8') as out:
        out.write("=" * 80 + "\n")
        out.write(f"АНАЛИЗ ФАЙЛОВ ПРОЕКТА\n")
        out.write(f"Директория: {base_dir}\n")
        out.write(f"Найдено файлов: {len(files)}\n")
        out.write("=" * 80 + "\n\n")
        
        for file_path in files:
            try:
                # Относительный путь для читаемости
                rel_path = file_path.relative_to(base_dir)
            except ValueError:
                rel_path = file_path
            
            out.write("\n" + "=" * 80 + "\n")
            out.write(f"ФАЙЛ: {rel_path}\n")
            out.write("=" * 80 + "\n\n")
            
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
                out.write(content)
                if not content.endswith('\n'):
                    out.write('\n')
            except UnicodeDecodeError:
                try:
                    with open(file_path, 'r', encoding='cp1251') as f:
                        content = f.read()
                    out.write(content)
                    if not content.endswith('\n'):
                        out.write('\n')
                except Exception as e:
                    out.write(f"[Ошибка чтения файла: {e}]\n")
            except Exception as e:
                out.write(f"[Ошибка чтения файла: {e}]\n")
            
            out.write("\n\n")
        
        out.write("=" * 80 + "\n")
        out.write("КОНЕЦ ФАЙЛА\n")
        out.write("=" * 80 + "\n")


def main():
    print("Сканирование директории...")
    files = collect_files()
    
    if not files:
        print("Файлы для обработки не найдены.")
        return
    
    print(f"Найдено файлов: {len(files)}")
    print("\nСписок файлов:")
    for f in files:
        try:
            print(f"  - {f.relative_to(Path.cwd())}")
        except ValueError:
            print(f"  - {f}")
    
    print("\nЗапись в all_api_files.txt...")
    write_files_content(files)
    print("Готово! Результат сохранён в all_api_files.txt")


if __name__ == '__main__':
    main()