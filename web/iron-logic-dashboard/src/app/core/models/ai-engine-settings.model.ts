export type AiModelId = 'gpt-4o' | 'claude-3-5-sonnet' | 'gemini-1-5-pro';

export interface AiEngineSettings {
  model: AiModelId;
  apiKey: string;
  baseUrl: string;
  maxTokensPerUserMonth: number;
  contextWindowSize: number;
  temperature: number;
  strictFormChecking: boolean;
  masterPrompt: string;
}

export const defaultAiEngineSettings: AiEngineSettings = {
  model: 'gpt-4o',
  apiKey: '',
  baseUrl: 'https://api.openai.com/v1',
  maxTokensPerUserMonth: 200000,
  contextWindowSize: 32000,
  temperature: 0.25,
  strictFormChecking: true,
  masterPrompt:
    "You are IronLogic's elite fitness coach. Prioritize safety, progressive overload, and evidence-based guidance. Keep feedback practical and concise.",
};
