import api from './api';

export interface LoginRequest {
  name: string;
  userPassword: string;
}

export interface LoginResponse {
  token: string;
  name: string;
  role: string;
}

export interface ApiResponse<T> {
  statusCode: number;
  message: string;
  data: T;
}

export interface RegisterRequest {
  name: string;
  passwordHash: string;
  role: string;
}

export const authApi = {
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
  try {
    const response = await api.post<any>('/auth/login', credentials);
    console.log('Full login response:', JSON.stringify(response.data, null, 2));
    
    // Backend returns Response<LoginResponseDto> with structure:
    // { statusCode: number, message: string, data: { token: string, name: string, role: string } }
    // ASP.NET Core uses camelCase by default for JSON
    
    const responseData = response.data;
    
    // Check for error status codes
    if (responseData.statusCode && responseData.statusCode !== 200) {
      throw new Error(responseData.message || 'Login failed');
    }
    
    // Check if data is nested in response.data.data (normal case)
    if (responseData.data && typeof responseData.data === 'object') {
      const loginData = responseData.data;
      // Handle both PascalCase (if serializer doesn't convert) and camelCase (default)
      const token = loginData.Token || loginData.token;
      const name = loginData.Name || loginData.name;
      const role = loginData.Role || loginData.role;
      
      if (token && name && role) {
        return { token, name, role };
      }
    }
    
    // Check if data is directly in response (unlikely but possible)
    if (responseData.Token || responseData.token) {
      return {
        token: responseData.Token || responseData.token,
        name: responseData.Name || responseData.name,
        role: responseData.Role || responseData.role,
      };
    }
    
    // If we get here, log the actual structure for debugging
    console.error('Unexpected response structure. Full response:', JSON.stringify(responseData, null, 2));
    throw new Error(`Invalid response format. Received: ${JSON.stringify(responseData)}`);
  } catch (error: any) {
    console.error('Login error details:', {
      message: error.message,
      response: error.response?.data,
      status: error.response?.status,
    });
    
    // Handle error responses from backend
    if (error.response?.data) {
      const errorData = error.response.data;
      // Check for nested error message
      const errorMessage = errorData.message || 
                          errorData.data?.message || 
                          (errorData.statusCode && errorData.statusCode !== 200 ? errorData.message : null) ||
                          'Login failed. Please check your credentials.';
      throw new Error(errorMessage);
    }
    
    // Re-throw if it's already an Error with a message
    if (error instanceof Error) {
      throw error;
    }
    
    throw new Error('Login failed. Please check your credentials.');
  }
},

  register: async (data: RegisterRequest): Promise<any> => {
    const response = await api.post('/auth/register', data);
    // Handle wrapped response if needed
    return response.data.data || response.data;
  },
};

