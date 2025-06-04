import csv
import mysql.connector
import tkinter as tk
from tkinter import filedialog
import os

#0. pip로 설치할 라이브러리
#  pip install mysql-connector-python
#  pip install tkinter

# ------------------------------
# 1. 데이터베이스 연결 정보 설정
# ------------------------------
db_config = {
    'host': 'localhost',        
    'user': 'root',
    'password': 'root',
    'database': 'ur_printer_local_copy'
}

# ------------------------------
# 2. 디렉토리 선택 창 (tkinter)
# ------------------------------
root = tk.Tk()
root.withdraw()  # 메인 윈도우 숨기기

selected_dir = filedialog.askdirectory(
    title="CSV 파일이 있는 폴더 선택"
)

if not selected_dir:
    print("폴더가 선택되지 않았습니다.")
    exit()

# 선택한 디렉토리 내의 CSV 파일 목록 가져오기
csv_files = [f for f in os.listdir(selected_dir) if f.lower().endswith('.csv')]

if not csv_files:
    print("선택한 폴더에 CSV 파일이 없습니다.")
    exit()

# ------------------------------
# 3. MariaDB 연결
# ------------------------------
conn = mysql.connector.connect(**db_config)
cursor = conn.cursor()

# ------------------------------
# 4. CSV 파일별로 데이터 INSERT
# ------------------------------

# 시리얼 번호 변환 매핑 딕셔너리
serial_prefix_map = {
    "136030": "501",
    "136020": "501",
    "136010": "501",
    "136040": "501",
    "136050": "501",
    "136060": "501",
    "136070": "501",
    "136080": "501",
    "136090": "501",
    # 필요 시 여기에 추가
}

for csv_file in csv_files:
    csv_file_path = os.path.join(selected_dir, csv_file)
    print(f"처리 중인 파일: {csv_file_path}")

    with open(csv_file_path, newline='', encoding='utf-8') as csvfile:
        csv_reader = csv.reader(csvfile)

        for row in csv_reader:
            # CSV 각 열에 대한 매핑
            # row[0] : order_number
            # row[1] : prd_date   (예: '250402')
            # row[2] : prd_line   (예: '1')
            # row[3] : order_place(예: '이천시')
            # row[4] : usage      (예: '음식물', '재사용', '일반')
            # row[5] : liter      (예: '1')
            # row[6] : serial     (예: '13603000000001')

            order_number = 502501
            prd_date = row[1]
            prd_line = row[2]
            order_place = row[3]
            usage = row[4]
            liter = row[5]
            serial_original = row[6]

            # 1. 시리얼 번호 변환
            serial_modified = serial_original
            for prefix, new_prefix in serial_prefix_map.items():
                if serial_original.startswith(prefix):
                    serial_modified = new_prefix + serial_original[len(prefix):]
                    break

            # 2. usage 매핑
            if usage == "음식물":
                usage_mapped = "food"
            elif usage == "재사용":
                usage_mapped = "recycle"
            elif usage == "일반":
                usage_mapped = "general"
            else:
                usage_mapped = usage

            # 3. INSERT 쿼리 실행
            sql = """
                  INSERT INTO production_data
                      (serial, prd_date, prd_line, order_place, `usage`, liter, order_number)
                  VALUES
                      (%s, %s, %s, %s, %s, %s, %s) \
                  """
            values = (
                serial_modified,
                prd_date,
                prd_line,
                order_place,
                usage_mapped,
                liter,
                order_number
            )

            cursor.execute(sql, values)

    conn.commit()
    print(f"{csv_file} 처리 완료")

# ------------------------------
# 5. 모든 CSV 처리 완료 후 정리
# ------------------------------
cursor.close()
conn.close()

print("모든 CSV 파일의 데이터가 성공적으로 DB에 저장되었습니다.")