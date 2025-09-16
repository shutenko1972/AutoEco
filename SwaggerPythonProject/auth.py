# -*- coding: utf-8 -*-
from flask import Flask, request, jsonify
from functools import wraps
import base64

app = Flask(__name__)

# �������� ������� ������ (username: password)
VALID_USERS = {
    'admin': 'admin123',
    'testuser': 'testpass',
    'demo': 'demo123'
}

def require_auth(f):
    @wraps(f)
    def decorated(*args, **kwargs):
        auth = request.authorization
        if not auth or not check_auth(auth.username, auth.password):
            return authenticate()
        return f(*args, **kwargs)
    return decorated

def check_auth(username, password):
    """��������� ������������ ������� ������"""
    return VALID_USERS.get(username) == password

def authenticate():
    """���������� ������ �� ��������������"""
    return jsonify({
        'message': 'Authentication required',
        'error': 'Unauthorized'
    }), 401, {'WWW-Authenticate': 'Basic realm="Login Required"'}

# ������� API � ���������������
@app.route('/api/protected')
@require_auth
def protected_resource():
    """���������� ������, ��������� ��������������"""
    return jsonify({
        'message': 'Welcome to protected resource!',
        'user': request.authorization.username,
        'status': 'authenticated'
    })

@app.route('/api/public')
def public_resource():
    """��������� ������, ��������� ��� ��������������"""
    return jsonify({
        'message': 'This is public information',
        'data': ['item1', 'item2', 'item3']
    })

@app.route('/api/login', methods=['POST'])
def login():
    """�������� ��� �����"""
    auth = request.authorization
    if not auth:
        return jsonify({'error': 'No credentials provided'}), 400
    
    if check_auth(auth.username, auth.password):
        return jsonify({
            'message': 'Login successful',
            'user': auth.username,
            'token': base64.b64encode(f"{auth.username}:{auth.password}".encode()).decode()
        })
    else:
        return jsonify({'error': 'Invalid credentials'}), 401

@app.route('/api/users')
@require_auth
def get_users():
    """�������� ������ ������������� (������� ��������������)"""
    return jsonify({
        'users': list(VALID_USERS.keys()),
        'message': 'Authenticated users list'
    })

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5001)