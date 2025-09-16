# -*- coding: utf-8 -*-
from flask import Flask, request, jsonify
from flask_restx import Api, Resource, fields
from flask_cors import CORS
from functools import wraps

app = Flask(__name__)
CORS(app)

# Тестовые учетные данные
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
            return jsonify({'error': 'Authentication required'}), 401
        return f(*args, **kwargs)
    return decorated

def check_auth(username, password):
    return VALID_USERS.get(username) == password

# Swagger configuration
api = Api(
    app,
    version='1.0',
    title='Users API with Auth',
    description='User Management API with Basic Authentication',
    doc='/swagger/',
    authorizations={
        'basicAuth': {
            'type': 'basic',
            'in': 'header',
            'name': 'Authorization'
        }
    },
    security='basicAuth'
)

# Data models
user_model = api.model('User', {
    'id': fields.Integer(readonly=True, description='User ID'),
    'name': fields.String(required=True, description='User name'),
    'username': fields.String(description='Username'),
    'email': fields.String(required=True, description='User email'),
    'phone': fields.String(description='Phone'),
    'website': fields.String(description='Website')
})

login_model = api.model('Login', {
    'username': fields.String(required=True, description='Username'),
    'password': fields.String(required=True, description='Password')
})

# Mock data
users = [
    {'id': 1, 'name': 'John Doe', 'username': 'johndoe', 'email': 'john@example.com', 'phone': '1-234-567-8901', 'website': 'johndoe.org'},
    {'id': 2, 'name': 'Jane Smith', 'username': 'janesmith', 'email': 'jane@example.com', 'phone': '1-987-654-3210', 'website': 'janesmith.org'}
]

@api.route('/login')
class Login(Resource):
    @api.expect(login_model)
    @api.doc(description='Authenticate user')
    def post(self):
        """User login"""
        data = api.payload
        username = data.get('username')
        password = data.get('password')
        
        if check_auth(username, password):
            return {
                'message': 'Login successful',
                'user': username,
                'authenticated': True
            }
        else:
            return {'error': 'Invalid credentials'}, 401

@api.route('/users')
class UsersList(Resource):
    @api.doc(security='basicAuth')
    @api.doc('list_users')
    @api.marshal_list_with(user_model)
    @require_auth
    def get(self):
        """Get all users (requires authentication)"""
        return users

    @api.doc(security='basicAuth')
    @api.expect(user_model)
    @api.marshal_with(user_model, code=201)
    @require_auth
    def post(self):
        """Create new user (requires authentication)"""
        data = api.payload
        new_id = max(user['id'] for user in users) + 1 if users else 1
        new_user = {
            'id': new_id,
            'name': data.get('name'),
            'username': data.get('username'),
            'email': data.get('email'),
            'phone': data.get('phone'),
            'website': data.get('website')
        }
        users.append(new_user)
        return new_user, 201

@api.route('/users/<int:user_id>')
class UserResource(Resource):
    @api.doc(security='basicAuth')
    @api.marshal_with(user_model)
    @require_auth
    def get(self, user_id):
        """Get user by ID (requires authentication)"""
        user = next((u for u in users if u['id'] == user_id), None)
        if not user:
            api.abort(404, f"User {user_id} not found")
        return user

    @api.doc(security='basicAuth')
    @api.expect(user_model)
    @api.marshal_with(user_model)
    @require_auth
    def put(self, user_id):
        """Update user (requires authentication)"""
        user = next((u for u in users if u['id'] == user_id), None)
        if not user:
            api.abort(404, f"User {user_id} not found")
        
        data = api.payload
        user.update({
            'name': data.get('name', user['name']),
            'username': data.get('username', user['username']),
            'email': data.get('email', user['email']),
            'phone': data.get('phone', user['phone']),
            'website': data.get('website', user['website'])
        })
        return user

    @api.doc(security='basicAuth')
    @require_auth
    def delete(self, user_id):
        """Delete user (requires authentication)"""
        global users
        user = next((u for u in users if u['id'] == user_id), None)
        if not user:
            api.abort(404, f"User {user_id} not found")
        
        users = [u for u in users if u['id'] != user_id]
        return {'message': f'User {user_id} deleted'}, 200

# Public endpoint (no auth required)
@app.route('/api/public')
def public_info():
    """Public information available without authentication"""
    return jsonify({
        'message': 'This is public data',
        'version': '1.0',
        'endpoints': {
            'public': '/api/public',
            'protected': '/users (requires auth)',
            'login': '/login (POST)'
        }
    })

if __name__ == '__main__':
    app.run(debug=True, host='0.0.0.0', port=5000)