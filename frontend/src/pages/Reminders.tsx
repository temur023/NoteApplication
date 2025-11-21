import React, { useState, useEffect } from 'react';
import { remindersApi, Reminder, ReminderCreateDto, ReminderUpdateDto } from '../api/reminders';
import { notesApi, Note } from '../api/notes';
import { Layout } from '../components/Layout';
import { format } from 'date-fns';

export const Reminders: React.FC = () => {
  const [reminders, setReminders] = useState<Reminder[]>([]);
  const [notes, setNotes] = useState<Note[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingReminder, setEditingReminder] = useState<Reminder | null>(null);
  const [formData, setFormData] = useState({ noteId: '', reminderTime: '' });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      setLoading(true);
      const [remindersResponse, notesResponse] = await Promise.all([
        remindersApi.getAll(),
        notesApi.getAll(),
      ]);
      setReminders(remindersResponse.data || []);
      setNotes(notesResponse.data || []);
    } catch (err: any) {
      setError('Failed to load reminders');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = () => {
    setEditingReminder(null);
    setFormData({ noteId: '', reminderTime: '' });
    setShowModal(true);
  };

  const handleEdit = (reminder: Reminder) => {
    setEditingReminder(reminder);
    setFormData({
      noteId: reminder.noteId.toString(),
      reminderTime: format(new Date(reminder.reminderTime), "yyyy-MM-dd'T'HH:mm"),
    });
    setShowModal(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this reminder?')) return;

    try {
      await remindersApi.delete(id);
      await loadData();
    } catch (err: any) {
      setError('Failed to delete reminder');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingReminder) {
        const updateDto: ReminderUpdateDto = {
          id: editingReminder.id,
          reminderTime: new Date(formData.reminderTime).toISOString(),
        };
        await remindersApi.update(updateDto);
      } else {
        const createDto: ReminderCreateDto = {
          noteId: parseInt(formData.noteId),
          reminderTime: new Date(formData.reminderTime).toISOString(),
        };
        await remindersApi.create(createDto);
      }
      setShowModal(false);
      await loadData();
    } catch (err: any) {
      setError('Failed to save reminder');
    }
  };

  const getNoteTitle = (noteId: number) => {
    const note = notes.find((n) => n.id === noteId);
    return note?.title || 'Unknown Note';
  };

  if (loading) {
    return (
      <Layout>
        <div className="flex justify-center items-center h-64">
          <div className="text-gray-500">Loading reminders...</div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="px-4 py-6 sm:px-0">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold text-gray-900">Reminders</h1>
          <button
            onClick={handleCreate}
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
          >
            + New Reminder
          </button>
        </div>

        {error && (
          <div className="mb-4 rounded-md bg-red-50 p-4">
            <div className="text-sm text-red-800">{error}</div>
          </div>
        )}

        {reminders.length === 0 ? (
          <div className="text-center py-12">
            <p className="text-gray-500 text-lg">No reminders yet. Create your first reminder!</p>
          </div>
        ) : (
          <div className="bg-white shadow overflow-hidden sm:rounded-md">
            <ul className="divide-y divide-gray-200">
              {reminders.map((reminder) => (
                <li key={reminder.id} className="px-6 py-4 hover:bg-gray-50">
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <p className="text-sm font-medium text-gray-900">
                        {getNoteTitle(reminder.noteId)}
                      </p>
                      <p className="text-sm text-gray-500">
                        {format(new Date(reminder.reminderTime), 'PPpp')}
                      </p>
                    </div>
                    <div className="flex space-x-2">
                      <button
                        onClick={() => handleEdit(reminder)}
                        className="px-3 py-1 text-sm font-medium text-primary-700 bg-primary-50 rounded-md hover:bg-primary-100"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => handleDelete(reminder.id)}
                        className="px-3 py-1 text-sm font-medium text-red-700 bg-red-50 rounded-md hover:bg-red-100"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          </div>
        )}

        {showModal && (
          <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
            <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
              <h3 className="text-lg font-bold text-gray-900 mb-4">
                {editingReminder ? 'Edit Reminder' : 'Create Reminder'}
              </h3>
              <form onSubmit={handleSubmit}>
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-700 mb-1">Note</label>
                  <select
                    required
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                    value={formData.noteId}
                    onChange={(e) => setFormData({ ...formData, noteId: e.target.value })}
                    disabled={!!editingReminder}
                  >
                    <option value="">Select a note</option>
                    {notes.map((note) => (
                      <option key={note.id} value={note.id}>
                        {note.title}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Reminder Time
                  </label>
                  <input
                    type="datetime-local"
                    required
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                    value={formData.reminderTime}
                    onChange={(e) => setFormData({ ...formData, reminderTime: e.target.value })}
                  />
                </div>
                <div className="flex space-x-2">
                  <button
                    type="button"
                    onClick={() => setShowModal(false)}
                    className="flex-1 px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="flex-1 px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700"
                  >
                    {editingReminder ? 'Update' : 'Create'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

