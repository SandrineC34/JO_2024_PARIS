from docx import Document

doc = Document("manuel.docx")

for para in doc.paragraphs:
    print(para.text)
