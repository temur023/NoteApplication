import api from './api';

export interface Note {
  id?: number;
  title: string;
  content: string;
  createdAt?: string;
}

export interface NoteCreateDto {
  title: string;
  content: string;
}

export interface NoteUpdateDto {
  id: number;
  title?: string;
  content?: string;
}

export interface NoteFilter {
  page?: number;
  pageSize?: number;
  search?: string;
}

export interface PagedResponse<T> {
  statusCode: number;
  message: string;
  data: T[];
  pageNumber?: number;
  pageSize?: number;
  totalRecords?: number;
}

export const notesApi = {
  getAll: async (filter?: NoteFilter): Promise<PagedResponse<Note>> => {
    const response = await api.get<PagedResponse<Note>>('/notes/get-all', { params: filter });
    // Backend returns PagedResponse which extends Response<List<T>>
    // The data is already in response.data.data, but we need to handle both formats
    if (Array.isArray(response.data.data)) {
      return {
        statusCode: response.data.statusCode || 200,
        message: response.data.message || '',
        data: response.data.data,
        pageNumber: response.data.pageNumber,
        pageSize: response.data.pageSize,
        totalRecords: response.data.totalRecords,
      };
    }
    // Fallback for direct array response
    return response.data;
  },

  getById: async (id: number): Promise<Note> => {
    const response = await api.get<{ statusCode: number; message: string; data: Note }>(`/notes/${id}`);
    return response.data.data || response.data as any;
  },

  create: async (note: NoteCreateDto): Promise<Note> => {
    const response = await api.post<{ statusCode: number; message: string; data: Note }>('/notes/create', note);
    return response.data.data || response.data as any;
  },

  update: async (note: NoteUpdateDto): Promise<Note> => {
    const response = await api.put<{ statusCode: number; message: string; data: Note }>('/notes/update', note);
    return response.data.data || response.data as any;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/notes/${id}`);
  },
};

