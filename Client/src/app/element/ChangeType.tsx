import moment, { Duration } from 'moment';
import React, { FC, useCallback, useEffect, useMemo, useState } from 'react';
import { useAsyncEffect } from 'use-async-effect';
import wretch from 'wretch';

import { createStyles, makeStyles } from '@material-ui/styles';

import { kenticoKontent } from '../../appSettings.json';
import { element as elementTerms } from '../../terms.en-us.json';
import { loadModule } from '../../utilities/modules';
import { Loading } from '../Loading';
import { IChangeTypeConfig } from '../shared/IChangeTypeConfig';
import { IChangeTypeResponse } from '../shared/IChangeTypeResponse';
import { IGetTypesResponse } from '../shared/IGetChangeTypeResponse';
import { IContext } from '../shared/models/customElement/IContext';
import { ICustomElement } from '../shared/models/customElement/ICustomElement';
import { IContentItem } from '../shared/models/management/IContentItem';
import { IContentType } from '../shared/models/management/IContentType';

// Expose access to Kentico custom element API
declare const CustomElement: ICustomElement<IChangeTypeConfig>;

const useStyles = makeStyles(() =>
  createStyles({
    row: { display: 'flex', flexDirection: 'row', margin: '4px 0' },
    fullWidthCell: { flex: 1 },
    select: {
      border: 'none',
      color: '#4c4d52',
    },
  })
);

type ElementType =
  | 'asset'
  | 'snippet'
  | 'custom'
  | 'date_time'
  | 'guidelines'
  | 'modular_content'
  | 'number'
  | 'multiple_choice'
  | 'rich_text'
  | 'taxonomy'
  | 'text'
  | 'url_slug';

const typeMap: { [key in ElementType]: ElementType[] } = {
  asset: ['asset'],
  snippet: ['snippet'],
  custom: ['custom'],
  date_time: ['date_time'],
  guidelines: ['guidelines', 'text'],
  modular_content: ['modular_content'],
  number: ['number'],
  multiple_choice: ['multiple_choice'],
  rich_text: ['rich_text'],
  taxonomy: ['taxonomy'],
  text: ['text', 'date_time', 'custom', 'number', 'rich_text'],
  url_slug: ['text', 'url_slug'],
};

export const ChangeType: FC = () => {
  const styles = useStyles();

  const [available, setAvailable] = useState(false);
  const [enabled, setEnabled] = useState(true);
  const [customElementConfig, setCustomElementConfig] = useState<IChangeTypeConfig | null>(null);
  const [customElementContext, setCustomElementContext] = useState<IContext | null>(null);
  const [loading, setLoading] = useState(false);
  const [loaded, setLoaded] = useState(false);

  const [currentType, setCurrentType] = useState<IContentType>();
  const [otherTypes, setOtherTypes] = useState<IContentType[]>();
  const [typeId, setTypeId] = useState('');
  const [elementMappings, setElementMappings] = useState<{ [key: string]: string }>({});

  const [totalApiCalls, setTotalApiCalls] = useState(0);
  const [totalTime, setTotalTime] = useState<Duration>();
  const [newItem, setNewItem] = useState<IContentItem>();
  const [updatedItems, setUpdatedItems] = useState<IContentItem[]>([]);

  const [error, setError] = useState<string>();

  useEffect(() => {
    if (!available) {
      const initCustomElement = (element: ICustomElement<IChangeTypeConfig>, context: IContext) => {
        setAvailable(true);
        setElementEnabled(!element.disabled);
        setCustomElementConfig(element.config);
        setCustomElementContext(context);

        CustomElement.onDisabledChanged((disabled) => setElementEnabled(!disabled));
      };

      const setElementEnabled = (enabled: boolean) => {
        setEnabled(enabled);
      };

      loadModule(kenticoKontent.customElementScriptEndpoint, () => CustomElement.init(initCustomElement));
    }
  }, [available]);

  useEffect(() => {
    if (available) {
      CustomElement.setHeight(document.documentElement.offsetHeight);
    }
  });

  useAsyncEffect(async () => {
    if (available && customElementConfig && customElementContext) {
      setLoading(true);

      const request = wretch(`${customElementConfig.getTypesEndpoint}/${customElementContext.item.codename}`)
        .get(elementMappings)
        .json<IGetTypesResponse>();

      try {
        const response = await request;

        setCurrentType(response.currentType);
        setOtherTypes(response.otherTypes);
      } catch (error) {
        setError(error.message);
      }

      setLoading(false);
    }
  }, [available, customElementConfig, customElementContext]);

  const selectedType = useMemo(() => otherTypes?.find((otherType) => otherType.id === typeId), [otherTypes, typeId]);

  useEffect(() => {
    if (selectedType && currentType) {
      setElementMappings(() => {
        let elementMappings: { [key: string]: string } = {};
        for (const element of selectedType.elements) {
          const currentTypeElement = currentType.elements.find(
            (currentElement) => currentElement.codename === element.codename
          );

          if (currentTypeElement) {
            elementMappings[element.id] = currentTypeElement.id;
          }
        }

        return { ...elementMappings };
      });
    }
  }, [selectedType, currentType]);

  const changeType = useCallback(async () => {
    if (customElementConfig && customElementContext) {
      setLoading(true);
      setLoaded(false);

      const request = wretch(
        `${customElementConfig.changeTypeEndpoint}/${customElementContext.item.codename}/${customElementContext.variant.codename}/${typeId}`
      )
        .post(elementMappings)
        .json<IChangeTypeResponse>();

      try {
        const response = await request;

        setTotalApiCalls(response.totalApiCalls);
        setTotalTime(moment.duration(response.totalMilliseconds));
        setNewItem(response.newItem);
        setUpdatedItems(response.updatedItems);
      } catch (error) {
        setError(error.message);
      }

      setLoading(false);
      setLoaded(true);
    }
  }, [customElementConfig, customElementContext, typeId, elementMappings]);

  const getTotalTimeString = useMemo(() => {
    if (totalTime) {
      let result = [];

      if (totalTime.hours() > 0) {
        result.push(`${totalTime.hours()} ${elementTerms.time.hours}`);
      }

      if (totalTime.minutes() > 0) {
        result.push(`${totalTime.minutes()} ${elementTerms.time.minutes}`);
      }

      if (totalTime.seconds() > 0) {
        result.push(`${totalTime.seconds() + totalTime.milliseconds() / 1000} ${elementTerms.time.seconds}`);
      }

      return result.join(', ');
    }
  }, [totalTime]);

  return (
    <div>
      {loading && <Loading />}
      {error && <div>{error}</div>}
      {error === undefined && available && enabled && currentType && otherTypes && (
        <>
          <div className={styles.row}>
            <div className={styles.fullWidthCell}>
              <p>{elementTerms.enabledDescription}</p>
            </div>
            <div>
              <button className='btn btn--primary btn--xs' disabled={selectedType === undefined} onClick={changeType}>
                {elementTerms.button}
              </button>
            </div>
          </div>
          <div className={styles.row}>
            <div className={styles.fullWidthCell}>
              <select className={styles.select} value={typeId} onChange={(event) => setTypeId(event.target.value)}>
                <option value=''>{elementTerms.chooseType}</option>
                {otherTypes.map((otherType) => (
                  <option key={otherType.id} value={otherType.id}>
                    {otherType.name ?? otherType.codename}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <div className={styles.row}>
            <div className={styles.fullWidthCell}>
              {typeId !== '' && currentType && selectedType && (
                <>
                  <div className={styles.row}>
                    <div className={styles.fullWidthCell}>
                      <b>{elementTerms.elementName}</b>
                    </div>
                    <div className={styles.fullWidthCell}>
                      <b>{elementTerms.elementType}</b>
                    </div>
                    <div className={styles.fullWidthCell}>
                      <b>{elementTerms.elementSource}</b>
                    </div>
                  </div>
                  {selectedType.elements.map((element) => (
                    <div key={element.id} className={styles.row}>
                      <div className={styles.fullWidthCell}>{element.name ?? element.codename}</div>
                      <div className={styles.fullWidthCell}>{element.type}</div>
                      <div className={styles.fullWidthCell}>
                        <select
                          className={styles.select}
                          value={elementMappings[element.id]}
                          onChange={(event) => {
                            const value = event.target.value;
                            if (value !== '') {
                              elementMappings[element.id] = value;
                              setElementMappings({ ...elementMappings });
                            } else {
                              delete elementMappings[element.id];
                              setElementMappings({ ...elementMappings });
                            }
                          }}
                        >
                          <option value=''>{elementTerms.chooseElement}</option>
                          {currentType.elements
                            .filter((currentElement) =>
                              typeMap[element.type as ElementType].some(
                                (allowedElementType) => allowedElementType === currentElement.type
                              )
                            )
                            .map((currentElement) => (
                              <option key={currentElement.id} value={currentElement.id}>
                                {`${currentElement.name ?? currentElement.codename} (${currentElement.type})`}
                              </option>
                            ))}
                        </select>
                      </div>
                    </div>
                  ))}
                </>
              )}
            </div>
          </div>
          {loaded && customElementContext && newItem && totalTime && (
            <>
              <div className={styles.row}>
                <div className={styles.fullWidthCell}>
                  <label className='content-item-element__label'>{elementTerms.newItem}</label>
                  <p>
                    <a
                      href={`https://app.kontent.ai/${customElementContext.projectId}/content-inventory/${customElementContext.variant.id}/content/${newItem.id}`}
                      target='_blank'
                      rel='noopener noreferrer'
                    >
                      {newItem.name}
                    </a>
                  </p>
                </div>
                <div className={styles.fullWidthCell}>
                  <label className='content-item-element__label'>{elementTerms.totalTime}</label>
                  <p>{getTotalTimeString}</p>
                </div>
                <div className={styles.fullWidthCell}>
                  <label className='content-item-element__label'>{elementTerms.totalApiCalls}</label>
                  <p>{totalApiCalls}</p>
                </div>
              </div>
              {updatedItems.length > 0 && (
                <div className={styles.row}>
                  <div className={styles.fullWidthCell}>
                    <label className='content-item-element__label'>{elementTerms.updatedItems}</label>
                    {updatedItems.map((item) => (
                      <p key={item.id}>
                        <a
                          href={`https://app.kontent.ai/${customElementContext.projectId}/content-inventory/${customElementContext.variant.id}/content/${item.id}`}
                          target='_blank'
                          rel='noopener noreferrer'
                        >
                          {item.name}
                        </a>
                      </p>
                    ))}
                  </div>
                </div>
              )}
            </>
          )}
        </>
      )}
      {error === undefined && !enabled && (
        <div className='content-item-element__guidelines'>
          <p>{elementTerms.disabledDescription}</p>
        </div>
      )}
    </div>
  );
};
