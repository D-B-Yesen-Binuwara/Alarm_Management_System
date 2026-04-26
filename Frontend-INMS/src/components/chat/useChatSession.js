import { useCallback, useEffect, useState } from 'react';
import ChatService from '../../services/ChatService';

const STORAGE_KEY = 'inms-chat-history';
const MAX_HISTORY_MESSAGES = 50;

export const suggestedPrompts = [
  'How many total nodes are there?',
  'How many active nodes are there?',
  'How many down nodes are there?',
  'Show active alarms',
  'Show critical alarms',
  'What is the status of SLBN-Colombo-01?',
  'Show impacted devices for alarm 1'
];

function normalizeMessages(messages) {
  return messages
    .filter(
      (message) =>
        message &&
        (message.type === 'user' || message.type === 'bot') &&
        typeof message.content === 'string' &&
        message.content.trim() !== ''
    )
    .slice(-MAX_HISTORY_MESSAGES);
}

function loadStoredMessages() {
  if (typeof window === 'undefined') {
    return [];
  }

  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return [];
    }

    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? normalizeMessages(parsed) : [];
  } catch {
    return [];
  }
}

export default function useChatSession() {
  const [messages, setMessages] = useState(() => loadStoredMessages());
  const [inputMessage, setInputMessage] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (typeof window === 'undefined') {
      return;
    }

    try {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(normalizeMessages(messages)));
    } catch {
      // Ignore storage write failures and keep the chat usable.
    }
  }, [messages]);

  const sendMessage = async (messageOverride) => {
    const userMessage = (messageOverride ?? inputMessage).trim();

    if (!userMessage || isLoading) {
      return;
    }

    setInputMessage('');
    setIsLoading(true);

    const nextUserMessage = { type: 'user', content: userMessage };
    setMessages((currentMessages) => normalizeMessages([...currentMessages, nextUserMessage]));

    try {
      const response = await ChatService.sendMessage(userMessage);
      const botMessage = {
        type: 'bot',
        content: response?.message?.trim() || "I couldn't generate a response right now."
      };

      setMessages((currentMessages) => normalizeMessages([...currentMessages, botMessage]));
    } catch {
      const errorMessage = {
        type: 'bot',
        content: 'Sorry, I encountered an error. Please try again.'
      };

      setMessages((currentMessages) => normalizeMessages([...currentMessages, errorMessage]));
    } finally {
      setIsLoading(false);
    }
  };

  const clearHistory = () => {
    setMessages([]);
  };

  const reloadHistory = useCallback(() => {
    setMessages(loadStoredMessages());
  }, []);

  return {
    messages,
    inputMessage,
    isLoading,
    setInputMessage,
    sendMessage,
    clearHistory,
    reloadHistory,
    suggestedPrompts
  };
}
