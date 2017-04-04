import math
import time
from hedgehog.client import connect
import socket

SPEED = 500

def forward():
    for num in range(0, 2):
        hedgehog.move(num, SPEED)

def back():
    for num in range(0, 2):
        hedgehog.move(num, -SPEED)

def left():
    hedgehog.move(0, -SPEED)
    hedgehog.move(1, SPEED)
        
def right():
    hedgehog.move(0, SPEED)
    hedgehog.move(1, -SPEED)

def stop():
    for num in range(0, 2):
        hedgehog.move(num, 0)

def increaseSpeed():
    if (SPEED < 1000):
        SPEED += 100

def decreaseSpeed():
    if (SPEED > 0):
        SPEED -= 100


# Main
with connect() as hedgehog:
    
    IP = socket.gethostbyname(socket.gethostname())
    PORT = 3131
    BUFFER_SIZE = 1
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind((IP, PORT))
    
    while True:
        print("Server listening at " + str(IP) + ":" + str(PORT))
        server.listen(5)
        
        connection, client_address = server.accept()
        print("[" + str(client_address) + "] Connected")
        
        while True:
            try:
                # Receive data
                print("[" + str(client_address) + "] Receiving Data...")
                dataBytes = connection.recv(BUFFER_SIZE)
                if not dataBytes: break
                
                # Convert received bytes into int
                data = int.from_bytes(dataBytes, byteorder='big')
                print("[" + str(client_address) + "] ...Received Data: " + str(data))
                
                # Do command
                if data == 0:
                   forward()
                elif data == 1:
                    left()
                elif data == 2:
                    back()
                elif data == 3:
                    right()
                elif data == 4:
                    increaseSpeed()
                elif data == 5:
                    decreaseSpeed()
                elif data == 6:
                    stop()
                elif data == 7:
                    stop()
                    
            except:
                print("[" + str(client_address) + "] Disconnected")
                connection.close()
                break
        break