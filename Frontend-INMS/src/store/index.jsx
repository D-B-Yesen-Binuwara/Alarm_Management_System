import { createContext, useContext, useMemo, useState } from 'react';

const AppStoreContext = createContext(null);

export const AppStoreProvider = ({ children }) => {
  const [selectedDeviceId, setSelectedDeviceId] = useState(null);

  const value = useMemo(
    () => ({ selectedDeviceId, setSelectedDeviceId }),
    [selectedDeviceId]
  );

  return <AppStoreContext.Provider value={value}>{children}</AppStoreContext.Provider>;
};

export const useAppStore = () => {
  const context = useContext(AppStoreContext);

  if (!context) {
    throw new Error('useAppStore must be used within AppStoreProvider');
  }

  return context;
};
