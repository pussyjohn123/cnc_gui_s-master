import cv2
import tensorflow as tf
from keras.preprocessing import image
import numpy as np
import os


def predict(img, tag=''):
    current_path = os.path.dirname(os.path.abspath(__file__))
    os.chdir(current_path)

    model_path = 'cnn_200_new.h5'
    model = tf.keras.models.load_model(model_path)

    # 圖片格式轉換
    img_array = image.img_to_array(img)
    img_array = np.expand_dims(img_array, axis=0)
    img_array /= 255.0  # 標準化處理

    # 預測類別
    predictions = model.predict(img_array)
    predicted_class = np.argmax(predictions, axis=1)
    print(predictions)
    print(f'{tag} Predicted class:{predicted_class[0] + 1}')
    return predicted_class[0] + 1
